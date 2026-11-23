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
package org.verapdf.io;

import org.verapdf.as.ASAtom;
import org.verapdf.as.exceptions.StringExceptions;
import org.verapdf.cos.*;
import org.verapdf.cos.xref.COSXRefInfo;
import org.verapdf.exceptions.InvalidPasswordException;
import org.verapdf.parser.DecodedObjectStreamParser;
import org.verapdf.parser.PDFParser;
import org.verapdf.parser.XRefReader;
import org.verapdf.pd.encryption.PDEncryption;
import org.verapdf.pd.encryption.StandardSecurityHandler;
import org.verapdf.tools.resource.FileResourceHandler;

import java.io.IOException;
import java.io.InputStream;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.logging.Level;
import java.util.logging.Logger;

/**
 * Resolves PDF indirect objects using cross-reference data and a
 * {@link PDFParser}.
 *
 * <p>Objects stored directly in the file are read at their recorded offsets;
 * compressed objects are decoded from object streams and cached for
 * subsequent lookups.</p>
 *
 * <h2>.NET porting notes</h2>
 * A C# port can retain this orchestration as a {@code PdfReader} class while
 * applying these direct mappings:
 * <ul>
 *   <li>{@link XRefReader} and {@link IReader} combine into an abstract base
 *       class plus an {@code IDisposable} interface or directly into an
 *       {@code IPdfReader : IDisposable} interface.</li>
 *   <li>{@link java.io.InputStream} maps to {@code System.IO.Stream};
 *       {@link SeekableInputStream} maps to a stream wrapper whose contract
 *       uses {@code Position}, {@code Length}, and {@code Seek(SeekOrigin)}.
 *       A .NET {@code FileStream} or seekable {@code MemoryStream} should be
 *       used directly where possible.</li>
 *   <li>{@link java.util.HashMap} maps to
 *       {@code Dictionary<long, DecodedObjectStreamParser>}, and
 *       {@link java.util.logging.Logger} maps to
 *       {@code Microsoft.Extensions.Logging.ILogger<PdfReader>} or an
 *       application-specific logging abstraction.</li>
 *   <li>{@link java.io.IOException} can generally map to
 *       {@code System.IO.IOException}, with a typed
 *       {@code InvalidPasswordException} retained for callers.</li>
 * </ul>
 * Prefer dependency injection for the parser/logger in the C# version and keep
 * parsing synchronous initially. A later asynchronous API should use
 * {@code IAsyncDisposable} and asynchronous stream reads as an end-to-end
 * change, rather than wrapping the synchronous parser in {@code Task.Run}.
 *
 * @author Timur Kamalov
 */
public class Reader extends XRefReader {

	/*
	 * C# mapping: PDFParser and PDFDocument become mutable private fields (or
	 * readonly constructor dependencies) and COSKey/COSObject become C# model
	 * types with corresponding nullability annotations.
	 */
	private static final Logger LOGGER = Logger.getLogger(Reader.class.getCanonicalName());

	private final PDFParser parser;
	private COSHeader header;
	private final Map<Long, DecodedObjectStreamParser> objectStreams;

	/**
	 * Creates a reader for a PDF file and initializes its header and
	 * cross-reference data. In C#, a path-based overload can delegate to the
	 * stream overload:
	 * {@code PdfReader(document, File.OpenRead(fileName), ownsStream: true)}.
	 * A stream implementation must therefore record whether this reader owns
	 * the stream so {@code Dispose()} has the same lifetime semantics.
	 *
	 * @param document low-level document that will receive resolved objects
	 * @param fileName path to the source PDF
	 * @throws IOException if the source cannot be read or initialized
	 */
	public Reader(final COSDocument document, final String fileName) throws IOException {
		super();
		this.parser = new PDFParser(document, fileName);
		this.objectStreams = new HashMap<>();
		init();
	}

	/**
	 * Creates a reader for PDF bytes supplied as a stream.
	 *
	 * <p>For the C# port, this overload is the core constructor. Use a
	 * {@code Stream} and document whether the caller or reader owns it;
	 * unlike this Java API, consider separate {@code leaveOpen} and
	 * {@code ownsStream} behavior instead of relying on convention.</p>
	 *
	 * @param document low-level document that will receive resolved objects
	 * @param fileStream source PDF bytes
	 * @throws IOException if the source cannot be read or initialized
	 */
	public Reader(final COSDocument document, final InputStream fileStream) throws IOException {
		super();
		this.parser = new PDFParser(document, fileStream);
		this.objectStreams = new HashMap<>();
		init();
	}

	//PUBLIC METHODS
	@Override
	public COSHeader getHeader() {
		return this.header;
	}

	@Override
	public COSObject getObject(final COSKey key) throws IOException {
		/*
		 * C# porting note: this is the primary xref resolver. Preserve the
		 * three-state offset convention:
		 *   0  => free/non-resolvable entry, > 0 => file offset,
		 *   < 0 => object stream number encoded as -offset.
		 * A .NET port can improve type safety with a discriminated union
		 * (record types) instead of reusing a signed long for these states.
		 */
		if (!super.containsKey(key)) {
			LOGGER.log(Level.FINE, "Trying to get object " + key.getNumber() + ' ' +
					key.getGeneration() + " that is not present in the document");
			return null;
		}
		long offset = getOffset(key);
		if (offset == 0) {
			return new COSObject();
		} else if (offset > 0) {
			if (header.getHeaderOffset() > 0) {
				offset += header.getHeaderOffset();
			}
			COSObject result = getObject(offset);
			result.setObjectKey(key);
			return result;
		}
		//TODO : set object key
		//a negative number to identify a case of object stream from normal offset
		//see method XrefStreamParser.parseStream and ISO 32000-2 7.5.7 and 7.5.8.3
		DecodedObjectStreamParser parser = objectStreams.get(-offset);
		if (parser != null) {
			return parser.getObject(key);
		}
		/*
		 * Resolve and cache the containing object stream lazily. In C#, use a
		 * Dictionary<long, DecodedObjectStreamParser> initially; add a lock or
		 * ConcurrentDictionary only if one Reader is intentionally shared
		 * across threads. A single parser stream is positionful and should not
		 * be read concurrently without synchronization.
		 */
		COSKey newKey = new COSKey(- (int)offset, 0);
		COSObject object = newKey.equals(key) ? null : getObject(newKey);
		if (object == null || object.getType() != COSObjType.COS_STREAM) {
			throw new IOException("Object number " + (-offset) + " should" +
					" be object stream, but in fact it is " +
					(object == null ? "null" : object.getType()));
		}
		COSStream objectStream = (COSStream) object.getDirectBase();
		parser = new DecodedObjectStreamParser(
				objectStream.getData(COSStream.FilterFlags.DECODE),
				objectStream, new COSKey((int) -offset, 0),
				this.parser.getDocument());
		objectStreams.put(-offset, parser);
		return parser.getObject(key);
	}

	@Override
	public COSObject getObject(final long offset) throws IOException {
		return this.parser.getObject(offset);
	}

	@Override
	public boolean isLinearized() {
		return this.parser.isLinearized();
	}

	@Override
	public SeekableInputStream getPDFSource() {
		// C# equivalent: expose the underlying System.IO.Stream only if callers
		// legitimately need seeking; otherwise keep it private to protect
		// mutable Position state.
		return this.parser.getPDFSource();
	}

	@Override
	public long getLastTrailerOffset() {
		long res = this.parser.getLastTrailerOffset();
		if (res == 0) {
			LOGGER.log(Level.FINE, "Offset of last trailer can not be determined");
		}
		return res;
	}

	@Override
	public COSObject getLinearizationDictionary() {
		return this.parser.getLinearizationDictionary();
	}

    @Override
    public void checkDocCanBeDecrypted() throws IOException {
		/*
		 * C# mapping: perform the password check synchronously and release all
		 * owned resources in a disposal path. Preserve the primary
		 * InvalidPasswordException; attach cleanup failures through the
		 * exception data/aggregate exception policy rather than silently
		 * replacing the password error.
		 */
        if (this.parser.isEncrypted() && !docCanBeDecrypted()) {
            InvalidPasswordException failure = new InvalidPasswordException(StringExceptions.ENCRYPTED_PDF);
            try {
                this.getPDFSource().close();
            } catch (IOException e) {
                failure.addSuppressed(e);
            }
            if (this.parser.getDocument() != null) {
                FileResourceHandler handler =
                        this.parser.getDocument().getResourceHandler();
                if (handler != null) {
                    try {
                        handler.close();
                    }  catch (IOException e) {
                        failure.addSuppressed(e);
                    }
                }
            }
            throw failure;
        }
    }

	// PRIVATE METHODS
	private void init() throws IOException {
		// C# porting note: initialization that can fail is usually kept in the
		// constructor or moved to a static CreateAsync factory when stream I/O
		// is asynchronous. Ensure constructor/factory failure disposes owned
		// streams, matching this try/catch.
		try {
			this.header = this.parser.getHeader();

			List<COSXRefInfo> infos = new ArrayList<>();
			this.parser.getXRefInfo(infos);
			setXRefInfo(infos);

		} catch (IOException e) {	// If exception is thrown in init() someone
			// should close document stream
			this.parser.closeInputStream();
			throw e;
		}
	}

	private boolean docCanBeDecrypted() {
		/*
		 * Encryption is deliberately checked against only the standard
		 * security handler. A .NET port should preserve this policy unless
		 * explicitly extended, and should represent failed parsing separately
		 * from a valid-but-wrong password if richer diagnostics are required.
		 */
		try {
			COSObject cosEncrypt = this.parser.getEncryption();
			if (cosEncrypt.isIndirect()) {
				cosEncrypt = this.parser.getObject(this.getOffset(cosEncrypt.getObjectKey()));
			}
			PDEncryption encryption = new PDEncryption(cosEncrypt);
			if (encryption.getFilter() != ASAtom.STANDARD) {
				return false;
			}
			StandardSecurityHandler ssh = new StandardSecurityHandler(encryption,
					this.getTrailer().getID(), this.parser.getDocument());
			boolean res = ssh.checkPassword();
			if (res) {
				this.parser.getDocument().setStandardSecurityHandler(ssh);
			}
			return res;
		} catch (IOException e) {
			LOGGER.log(Level.FINE, "Cannot read object " + this.parser.getEncryption().getObjectKey());
			return false;
		}
	}

	@Override
	public COSObject getLastXrefStream() {
		return parser.getLastXRefStream();
	}

	@Override
	public boolean isContainsXRefStream() {
		return parser.isContainsXRefStream();
	}

	@Override
	public int getGreatestKeyNumberFromXref() {
		int res = 1;
		for (COSKey key : this.getKeys()) {
			if (key.getNumber() > res) {
				res = key.getNumber();
			}
		}
		return res;
	}

	@Override
	public void close() throws IOException {
		/*
		 * C# mapping: implement IDisposable.Dispose()/IAsyncDisposable.DisposeAsync().
		 * Keep disposal idempotent and release every owned cached parser and the
		 * source parser/stream according to the constructor's ownership policy.
		 * The current Java implementation closes cached object-stream parsers;
		 * callers own the other lifecycle behavior provided by the parser layer.
		 */
		if (objectStreams != null) {
			for (Map.Entry<Long, DecodedObjectStreamParser> entry : this.objectStreams.entrySet()) {
				entry.getValue().closeInputStream();
			}
		}
	}
}
