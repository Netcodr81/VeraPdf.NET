/*
 * This file is part of veraPDF Parser, a module of the veraPDF project.
 * Copyright (c) 2015-2026, veraPDF Consortium <info@verapdf.org>
 * All rights reserved.
 *
 * veraPDF Parser is free software: you can redistribute it and/or modify
 * it under the terms of either:
 *
 * The GNU General public license GPLv3+.
 * You should have received a copy of the GNU General Public License
 * along with veraPDF Parser as the LICENSE.GPL file in the root of the source
 * tree.  If not, see http://www.gnu.org/licenses/ or
 * https://www.gnu.org/licenses/gpl-3.0.en.html.
 *
 * The Mozilla Public License MPLv2+.
 * You should have received a copy of the Mozilla Public License along with
 * veraPDF Parser as the LICENSE.MPL file in the root of the source tree.
 * If a copy of the MPL was not distributed with this file, you can obtain one at
 * http://mozilla.org/MPL/2.0/.
 */
package org.verapdf.cos;

import org.verapdf.as.ASAtom;
import org.verapdf.as.filters.io.ASBufferedInFilter;
import org.verapdf.cos.visitor.Writer;
import org.verapdf.cos.xref.COSXRefTable;
import org.verapdf.exceptions.LoopedException;
import org.verapdf.exceptions.ResourceLimitExceededException;
import org.verapdf.exceptions.VeraPDFParserException;
import org.verapdf.io.IReader;
import org.verapdf.io.InternalInputStream;
import org.verapdf.io.Reader;
import org.verapdf.io.SeekableInputStream;
import org.verapdf.io.TempFileHandler;
import org.verapdf.pd.PDDocument;
import org.verapdf.pd.encryption.StandardSecurityHandler;
import org.verapdf.tools.resource.ASFileStreamCloser;
import org.verapdf.tools.resource.FileResourceHandler;

import java.io.File;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.util.*;
import java.util.logging.Level;
import java.util.logging.Logger;

/**
 * The low-level representation of a PDF document.
 *
 * <p>This class owns the PDF's cross-reference table, trailer, header, and
 * {@link COSObject} body. Objects are loaded from the source on demand and
 * retained in the body after they are read. The higher-level
 * {@link PDDocument} API builds PDF concepts such as pages and fonts on top
 * of this representation.</p>
 *
 * <p>For an existing PDF, {@code COSDocument} also tracks newly added and
 * modified objects so {@link #saveTo(OutputStream)} can write an incremental
 * update rather than rewriting the original bytes.</p>
 *
 *
 * <h2>.NET porting notes</h2>
 * In C#, {@code COSDocument} is the low-level document aggregate and object lifetime boundary.
 * It should own indexed COS objects and disposable input state, offer an immutable
 * header/trailer view, and implement {@code IDisposable}; higher-level PD wrappers should depend
 * on it rather than duplicate parsing state.
 *
 * @author Timur Kamalov
 */
public class COSDocument {

	private static final Logger LOGGER = Logger.getLogger(COSDocument.class.getCanonicalName());

	private static volatile int maxNumberOfObjects = -1;

	private PDDocument doc;
	private IReader reader;
	private COSHeader header;
	private COSBody body;
	private COSXRefTable xref;
	private COSTrailer trailer;
	private COSTrailer firstTrailer;
	private COSTrailer lastTrailer;
	private boolean linearized;
	private boolean isNew;
	private StandardSecurityHandler standardSecurityHandler;
	private List<COSObject> changedObjects;
	private List<COSObject> addedObjects;
	private final FileResourceHandler resourceHandler;
	private String fileName;

	private long fileSize;
	private byte postEOFDataSize;

	private boolean xrefEOLMarkersComplyPDFA = true;
	private boolean subsectionHeaderSpaceSeparated = true;

	/**
	 * Creates an empty document model for constructing a new PDF.
	 *
	 * @param document the high-level document that owns this low-level model
	 */
	public COSDocument(final PDDocument document) {
		this.doc = document;
		this.header = new COSHeader();
		this.body = new COSBody();
		this.xref = new COSXRefTable();
		this.trailer = new COSTrailer();
		this.firstTrailer = new COSTrailer();
		this.lastTrailer = new COSTrailer();
		this.linearized = false;
		this.isNew = true;
		this.changedObjects = new ArrayList<>();
		this.addedObjects = new ArrayList<>();
		this.resourceHandler = new FileResourceHandler();
	}

	/**
	 * Opens a PDF from a file and initializes its header, cross-reference
	 * table, and trailer.
	 *
	 * @param fileName path to the PDF source
	 * @param document the high-level document that owns this low-level model
	 * @throws IOException if the source cannot be opened or its initial
	 *                     structures cannot be read
	 */
	public COSDocument(final String fileName, final PDDocument document) throws IOException {
		this.resourceHandler = new FileResourceHandler();
		this.fileName = fileName;
		initReader(fileName);

		initCOSDocument(document);
	}

	/**
	 * Opens a PDF from an input stream and initializes its document structures.
	 *
	 * @param fileStream stream containing the PDF
	 * @param document the high-level document that owns this low-level model
	 * @throws IOException if the source cannot be read or initialized
	 */
	public COSDocument(final InputStream fileStream, final PDDocument document) throws IOException {
		this.resourceHandler = new FileResourceHandler();
		initReader(fileStream);

		initCOSDocument(document);
	}

	private void initCOSDocument(final PDDocument document) throws IOException {
		this.doc = document;
		this.body = new COSBody();
        this.reader.checkDocCanBeDecrypted();

		this.header = this.reader.getHeader();
		this.xref = new COSXRefTable();
		this.xref.set(this.reader.getKeys());
		this.trailer = reader.getTrailer();
		this.firstTrailer = reader.getFirstTrailer();
		this.lastTrailer = reader.getLastTrailer();
		this.linearized = reader.isLinearized();
		this.changedObjects = new ArrayList<>();
		this.addedObjects = new ArrayList<>();
	}

	private void initReader(final InputStream fileStream) throws IOException {
		this.reader = new Reader(this, fileStream);
		this.resourceHandler.addResource(this.reader);
	}

	private void initReader(final String fileName) throws IOException {
		this.reader = new Reader(this, fileName);
		this.resourceHandler.addResource(this.reader);
	}

	public boolean isNew() {
		return this.isNew;
	}

	public void setHeader(String header) {
		this.header.setHeader(header);
	}

	/**
	 * Sets an upper bound on the number of indirect objects the document may declare in its
	 * cross-reference table. Enumerating the objects materialises one entry per key, so a document that
	 * declares an extreme number of objects (a small compressed input can, via an object stream) can
	 * exhaust the heap; with a bound set it is rejected with a {@link ResourceLimitExceededException}
	 * (a {@link VeraPDFParserException} subclass, so existing catches keep working) before the objects
	 * are materialised, instead of failing with an OutOfMemoryError. A negative value (the default)
	 * removes the bound, keeping the historical behaviour.
	 *
	 * @param max maximum number of indirect objects, or a negative value for no bound
	 */
	public static void setMaxNumberOfObjects(int max) {
		maxNumberOfObjects = max;
	}

	/**
	 * @return the current upper bound on the number of indirect objects, or {@code -1} if unbounded
	 */
	public static int getMaxNumberOfObjects() {
		return maxNumberOfObjects;
	}

	private void checkObjectCountLimit() {
		if (maxNumberOfObjects >= 0) {
			int declared = this.xref.getAllKeys().size();
			if (declared > maxNumberOfObjects) {
				throw new ResourceLimitExceededException("Number of indirect objects (" + declared
						+ ") exceeds the configured maximum of " + maxNumberOfObjects);
			}
		}
	}

	/**
	 * Resolves and returns the indirect objects declared by the cross-reference
	 * table. Objects already present in the in-memory body are reused.
	 *
	 * @return the readable indirect objects in this document
	 * @throws ResourceLimitExceededException if the configured object-count
	 *                                         limit is exceeded
	 */
	public List<COSObject> getObjects() {
		checkObjectCountLimit();
		List<COSObject> result = new ArrayList<>();
		for (COSKey key : this.xref.getAllKeys()) {
			COSObject obj = this.body.get(key);
			if (!obj.empty()) {
				result.add(obj);
			} else {
				try {
					COSObject newObj = this.reader.getObject(key);

					this.body.set(key, newObj);
					result.add(newObj);
				} catch (IOException e) {
					LOGGER.log(Level.FINE, "Error while parsing object : " + key.getNumber() +
							' ' + key.getGeneration());
				} catch (StackOverflowError e) {
					// TODO: double check this StackOverflow catching
					throw new LoopedException("Loop in getting object from reader", e);
				}
			}
		}
		return result;
	}

	/**
	 * Resolves the document's indirect objects and selects dictionaries whose
	 * {@code /Type} entry matches the requested PDF name.
	 *
	 * @param type PDF name to match against each object's {@code /Type} entry
	 * @return matching objects
	 * @throws ResourceLimitExceededException if the configured object-count
	 *                                         limit is exceeded
	 */
	public List<COSObject> getObjectsByType(ASAtom type) {
		checkObjectCountLimit();
		List<COSObject> result = new ArrayList<>();
		for (COSKey key : this.xref.getAllKeys()) {
			COSObject obj = this.body.get(key);
			if (!obj.empty()) {
				addObjectWithTypeKeyCheck(result, obj, type);
			} else {
				try {
					COSObject newObj = this.reader.getObject(key);

					this.body.set(key, newObj);
					addObjectWithTypeKeyCheck(result, obj, type);
				} catch (IOException e) {
					LOGGER.log(Level.FINE, "Error while parsing object : " + key.getNumber() +
							' ' + key.getGeneration());
				}
			}
		}
		return result;
	}

	private static void addObjectWithTypeKeyCheck(List<COSObject> objects,
												  COSObject obj, ASAtom type) {
		if (obj != null && !obj.empty() && obj.getType().isDictionaryBased()) {
			ASAtom actualType = obj.getNameKey(ASAtom.TYPE);
			if (actualType == type) {
				objects.add(obj);
			}
		}
	}

	/**
	 * Resolves the document's indirect objects and returns them indexed by
	 * their object and generation numbers.
	 *
	 * @return a map of indirect object keys to their resolved objects
	 * @throws ResourceLimitExceededException if the configured object-count
	 *                                         limit is exceeded
	 */
	public Map<COSKey, COSObject> getObjectsMap() {
		checkObjectCountLimit();
		Map<COSKey, COSObject> result = new HashMap<>();
		for (COSKey key : this.xref.getAllKeys()) {
			COSObject obj = this.body.get(key);
			if (!obj.empty()) {
				result.put(key, obj);
			} else {
				try {
					COSObject newObj = this.reader.getObject(key);

					this.body.set(key, newObj);
					result.put(key, newObj);
				} catch (IOException e) {
					LOGGER.log(Level.FINE, "Error while parsing object : " + key.getNumber() +
							' ' + key.getGeneration());
				}
			}
		}
		return result;
	}

	/**
	 * Parses an indirect object at its byte offset in the source PDF.
	 *
	 * @param offset byte offset at which the object begins
	 * @return the parsed object
	 * @throws VeraPDFParserException if parsing at the offset fails
	 */
	public COSObject getObject(final long offset) {
		try {
			return reader.getObject(offset);
		} catch (IOException e) {
			throw new VeraPDFParserException("Error while parsing object at offset " + offset, e);
		}
	}

	/**
	 * Retrieves an indirect object by key, parsing and caching it on first
	 * access.
	 *
	 * @param key object number and generation number to resolve
	 * @return the resolved object, or an empty object if the reader returns
	 *         no object for the key
	 * @throws VeraPDFParserException if parsing the object fails
	 */
	public COSObject getObject(final COSKey key) {
		try {
			COSObject obj = this.body.get(key);
			if (!obj.empty()) {
				return obj;
			}

			COSObject newObj = this.reader.getObject(key);
			if (newObj == null) {
				return new COSObject();
			}
			this.body.set(key, newObj);
			return this.body.get(key);
		} catch (IOException e) {
			//TODO : maybe not runtime, maybe no exception at all
			throw new VeraPDFParserException("Error while parsing object : " + key.getNumber() +
					' ' + key.getGeneration(), e);
		}
	}

	public Long getOffset(final COSKey key) {
		return this.reader.getOffset(key);
	}

	/**
	 * Places an object in the in-memory body under a specified indirect key
	 * and makes that key part of the cross-reference table.
	 *
	 * @param key indirect object key
	 * @param obj object to store
	 */
	public void setObject(final COSKey key, final COSObject obj) {
		this.body.set(key, obj);
		this.xref.newKey(key);
	}

	/**
	 * Adds an object to the document, assigning it the next available key if it
	 * does not already have one.
	 *
	 * @param obj object to register
	 * @return the object's indirect key
	 */
	public COSKey setObject(COSObject obj) {
		COSKey key = obj.getKey();

		//TODO : fix this method for document save
		if (key == null) {
			key = this.xref.next();
			this.body.set(key, obj.isIndirect() ? obj.getDirect() : obj);
		}

		this.xref.newKey(key);
		return key;
	}

	public COSTrailer getTrailer() {
		return this.trailer;
	}

	public COSTrailer getFirstTrailer() {
		return firstTrailer;
	}

	public COSTrailer getLastTrailer() {
		return lastTrailer;
	}

	public boolean isLinearized() {
		return linearized;
	}

	public PDDocument getPDDocument() {
		return this.doc;
	}

	public COSHeader getHeader() {
		return header;
	}

	public void setHeader(COSHeader header) {
		this.header = header;
	}

	public long getFileSize() {
		return fileSize;
	}

	public void setFileSize(long fileSize) {
		this.fileSize = fileSize;
	}

	public byte getPostEOFDataSize() {
		return postEOFDataSize;
	}

	public void setPostEOFDataSize(byte postEOFDataSize) {
		this.postEOFDataSize = postEOFDataSize;
	}

	public boolean isXrefEOLMarkersComplyPDFA() {
		return xrefEOLMarkersComplyPDFA;
	}

	public void setXrefEOLMarkersComplyPDFA(boolean xrefEOLMarkersComplyPDFA) {
		this.xrefEOLMarkersComplyPDFA = xrefEOLMarkersComplyPDFA;
	}

	public boolean isSubsectionHeaderSpaceSeparated() {
		return subsectionHeaderSpaceSeparated;
	}

	public void setSubsectionHeaderSpaceSeparated(boolean subsectionHeaderSpaceSeparated) {
		this.subsectionHeaderSpaceSeparated = subsectionHeaderSpaceSeparated;
	}

	/**
	 * Placeholder for a full-save operation; currently performs no work.
	 */
	public void save() {
		//TODO : implement this
	}

	public SeekableInputStream getPDFSource() {
		return this.reader.getPDFSource();
	}

	/**
	 * Writes this document's current header, objects, trailer, and cross-
	 * reference information through the supplied writer.
	 *
	 * @param writer writer configured for the destination
	 */
	public void saveAs(final Writer writer) {
		writer.writeHeader(this.header.getHeader());

		writer.addToWrite(this.xref.getAllKeys());
		writer.writeBody();

		writer.setTrailer(this.trailer);

		writer.writeXRefInfo();

		writer.clear();
	}

	/**
	 * Appends an incremental update to the original PDF bytes and copies the
	 * result to the supplied stream. Only objects registered as added or
	 * changed are emitted in the update.
	 *
	 * @param stream destination for the original document and incremental
	 *               update
	 */
	public void saveTo(final OutputStream stream) {
		File temp = null;
		try {
			temp = TempFileHandler.createTempFile("tmp_pdf_file", ".pdf");
			Writer pdfWriter = new Writer(this, temp.getAbsolutePath(),
					this.getPDFSource().getStreamLength());
			pdfWriter.writeIncrementalUpdate(changedObjects, addedObjects);
			pdfWriter.close();
			this.getPDFSource().reset();
			writeInputIntoOutput(this.getPDFSource(), stream);
			InternalInputStream pdf = new InternalInputStream(temp.getAbsolutePath());
			writeInputIntoOutput(pdf, stream);
			pdf.close();
		} catch (IOException e) {
			LOGGER.log(Level.FINE, "Can't write COSDocument to stream");
		} finally {
			if (temp != null && !temp.delete()) {
				temp.deleteOnExit();
			}
		}
	}

	private static void writeInputIntoOutput(InputStream input, OutputStream output) throws IOException {
		byte[] buf = new byte[ASBufferedInFilter.BF_BUFFER_SIZE];
		int read = input.read(buf, 0, buf.length);
		while (read != -1) {
			output.write(buf, 0, read);
			read = input.read(buf, 0, buf.length);
		}
	}

	public void setStandardSecurityHandler(StandardSecurityHandler standardSecurityHandler) {
		this.standardSecurityHandler = standardSecurityHandler;
	}

	public StandardSecurityHandler getStandardSecurityHandler() {
		return standardSecurityHandler;
	}

	public boolean isEncrypted() {
		return this.standardSecurityHandler != null;
	}

	public COSArray getID() {
		if (trailer != null) {
			COSObject res = trailer.getKey(ASAtom.ID);
			if (res.getType() == COSObjType.COS_ARRAY) {
				return (COSArray) res.getDirectBase();
			}
		}
		return null;
	}

	/**
	 * Marks a new indirect object for inclusion in the next incremental save.
	 * The same object instance is not added more than once.
	 *
	 * @param obj newly created object
	 */
	public void addObject(COSObject obj) {
		if (obj != null && !obj.empty() && !isObjectAdded(obj)) {
			this.addedObjects.add(obj);
		}
	}

	/**
	 * Removes an object from the pending additions for an incremental save.
	 *
	 * @param obj object to remove
	 */
	public void removeAddedObject(COSObject obj) {
		this.addedObjects.remove(obj);
	}

	/**
	 * Marks an existing object as modified for the next incremental save.
	 * The same object instance is not added more than once.
	 *
	 * @param obj modified object
	 */
	public void addChangedObject(COSObject obj) {
		if (obj != null && !obj.empty() && !isObjectChanged(obj)) {
			this.changedObjects.add(obj);
		}
	}

	/**
	 * Removes an object from the pending modifications for an incremental save.
	 *
	 * @param obj object to remove
	 */
	public void removeChangedObject(COSObject obj) {
		this.changedObjects.remove(obj);
	}

	/**
	 * Checks whether this exact object instance is pending addition.
	 *
	 * @param obj object to check
	 * @return {@code true} if the instance is in the added-object list
	 */
	public boolean isObjectAdded(COSObject obj) {
		return listContainsObject(addedObjects, obj);
	}

	/**
	 * Checks whether this exact object instance is pending modification or
	 * addition.
	 *
	 * @param obj object to check
	 * @return {@code true} if the instance is pending in either list
	 */
	public boolean isObjectChanged(COSObject obj) {
		return listContainsObject(changedObjects, obj) ||
				listContainsObject(addedObjects, obj);
	}

	private static boolean listContainsObject(List<COSObject> list, COSObject obj) {
		for (COSObject listObject : list) {
			if (listObject == obj) {
				return true;
			}
		}
		return false;
	}

	public long getLastTrailerOffset() {
		return this.reader.getLastTrailerOffset();
	}

	public COSObject getLastXRefStream() {
		return reader.getLastXrefStream();
	}

	public boolean isContainsXRefStream() {
		return reader.isContainsXRefStream();
	}

	public COSObject getLinearizationDictionary() {
		return reader.getLinearizationDictionary();
	}

	public List<COSObject> getObjectStreamsList() {
		return reader.getObjectStreamsList();
	}

	public int getLastKeyNumber() {
		return this.reader.getGreatestKeyNumberFromXref();
	}

	public boolean isReaderInitialized() {
		return this.reader != null;
	}

	public void addFileResource(ASFileStreamCloser resource) {
		this.resourceHandler.addResource(resource);
	}

	public FileResourceHandler getResourceHandler() {
		return resourceHandler;
	}

	public SortedSet<Long> getStartXRefs() {
		return this.reader.getStartXRefs();
	}

	public String getFileName() {
		return fileName;
	}
}
