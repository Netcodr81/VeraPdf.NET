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
import org.verapdf.as.filters.ASInFilter;
import org.verapdf.as.io.ASInputStream;
import org.verapdf.as.io.ASMemoryInStream;
import org.verapdf.as.io.ASOutputStream;
import org.verapdf.cos.visitor.ICOSVisitor;
import org.verapdf.cos.visitor.IVisitor;
import org.verapdf.io.InternalInputStream;
import org.verapdf.io.InternalOutputStream;
import org.verapdf.io.SeekableInputStream;
import org.verapdf.tools.TypeConverter;

import java.io.File;
import java.io.IOException;
import java.nio.charset.StandardCharsets;
import java.util.*;
import java.util.logging.Level;
import java.util.logging.Logger;

/**
 * A PDF stream: a dictionary of stream attributes paired with encoded or raw
 * byte data. Stream data access can expose the stored bytes or apply its
 * declared filters.
 *
 * <h2>.NET porting notes</h2>
 * In C#, port {@code COSStream} as a dictionary model plus a {@code Stream}/{@code MemoryStream}
 * payload. Track whether a caller or the stream object owns each payload, expose decoded/raw
 * views through separate methods, and implement {@code IDisposable} without closing a
 * caller-owned stream unintentionally.
 */
public class COSStream extends COSDictionary {

	public static final String STREAM = "stream";

	private static final Logger LOGGER = Logger.getLogger(COSStream.class.getCanonicalName());

	private ASInputStream stream;
	private FilterFlags flags;

	private boolean streamKeywordCRLFCompliant = true;
	private boolean endstreamKeywordCRLFCompliant = true;
	private long realStreamSize;

	protected COSStream() {
		super();
		this.flags = FilterFlags.RAW_DATA;

		setIndirectLength(0);
	}

	protected COSStream(final ASInputStream stream) {
		super();
		this.stream = stream;
		this.flags = FilterFlags.RAW_DATA;

		setIndirectLength(0);
	}

	protected COSStream(final String string) {
		super();
		this.stream = new ASMemoryInStream(string.getBytes(StandardCharsets.ISO_8859_1));
		this.flags = FilterFlags.RAW_DATA;
	}

	protected COSStream(final COSDictionary dictionary) {
		super(dictionary);
	}

	protected COSStream(final COSDictionary dictionary, final ASInputStream stream, final FilterFlags flags) {
		super(dictionary);
		this.stream = stream;
		this.flags = flags;
	}

	protected COSStream(final COSDictionary dictionary, final String string, final FilterFlags flags) {
		super(dictionary);
		this.stream = new ASMemoryInStream(string.getBytes(StandardCharsets.ISO_8859_1));
		this.flags = flags;
	}

	public static COSObject construct() {
		return new COSObject(new COSStream());
	}

	public static COSObject construct(final ASInputStream stream) {
		return new COSObject(new COSStream(stream));
	}

	public static COSObject construct(final String string) {
		return new COSObject(new COSStream(string));
	}

	public static COSObject construct(final COSDictionary dictionary) {
		return new COSObject(new COSStream(dictionary));
	}

	public static COSObject construct(final COSDictionary dictionary, final ASInputStream stream) {
		return construct(dictionary, stream, FilterFlags.RAW_DATA);
	}

	public static COSObject construct(final COSDictionary dictionary, final ASInputStream stream, final FilterFlags flags) {
		return new COSObject(new COSStream(dictionary, stream, flags));
	}

	public static COSObject construct(final COSDictionary dictionary, final String string) {
		return construct(dictionary, string, FilterFlags.RAW_DATA);
	}

	public static COSObject construct(final COSDictionary dictionary, final String string, final FilterFlags flags) {
		return new COSObject(new COSStream(dictionary, string, flags));
	}

	@Override
	public COSObjType getType() {
		return COSObjType.COS_STREAM;
	}

	@Override
	public void accept(final IVisitor visitor) {
		visitor.visitFromStream(this);
	}

	@Override
	public Object accept(final ICOSVisitor visitor) {
		return visitor.visitFromStream(this);
	}

	@Override
	public ASInputStream getData() {
		return getData(FilterFlags.RAW_DATA);
	}

	/**
	 * Reads the stream's decoded bytes into a COS string.
	 *
	 * @return a string containing the stream bytes, or an empty string if reading
	 * fails
	 */
	public COSString getStringFromTextStream() {
		try {
			return (COSString)COSString.construct(TypeConverter.inputStreamToByteArray(getData())).get();
		} catch (IOException e) {
			LOGGER.log(Level.WARNING, "Exception during creation string from stream (object key = " + getKey() + ')');
			return new COSString();
		}
	}

	@Override
	/**
	 * Opens the stream data using the requested representation. Raw data is
	 * returned for {@link FilterFlags#RAW_DATA}; when raw bytes are stored, other
	 * flags request decoding through the stream's declared filters.
	 *
	 * @param filterFlags requested data representation
	 * @return stream over the requested data
	 */
	public ASInputStream getData(final FilterFlags filterFlags) {
		try {
			if (filterFlags == FilterFlags.RAW_DATA || this.flags != FilterFlags.RAW_DATA) {
				this.stream.reset();
				return ASInputStream.createStreamFromStream(this.stream);
			}
			ASInputStream result = getFilters().getInputStream(
					ASInputStream.createStreamFromStream(stream),
					this.getKey(ASAtom.DECODE_PARMS));
			result.reset();
			return result;
		} catch (IOException e) {
			LOGGER.log(Level.FINE, "Can't get stream data");
			return new ASInFilter(this.stream) {
				@Override
				public int read() {
					return -1;
				}
			};
		}
	}

	@Override
	/**
	 * Stores decoded input, applying the stream's current filters when present.
	 *
	 * @param stream decoded data to store
	 * @return whether the encoded data could be stored
	 */
	public boolean setData(final ASInputStream stream) {
		COSFilters filters = getFilters();
		if (filters.empty()) {
			return setData(stream, FilterFlags.RAW_DATA);
		}
		try (InternalOutputStream fileWithData = InternalOutputStream.getInternalOutputStream()) {
			ASOutputStream encoder = filters.getOutputStream(fileWithData);
			encoder.write(stream);
			File encodedDataFile = fileWithData.getFile();
			return setData(new InternalInputStream(encodedDataFile, true), FilterFlags.RAW_DATA);
		} catch (IOException e) {
			LOGGER.log(Level.FINE, "Can not set data");
			return false;
		}
	}

	@Override
	/**
	 * Stores input together with a flag describing whether it is raw, decoded,
	 * decrypted, or both decoded and decrypted.
	 *
	 * @param stream input data
	 * @param flags representation of the supplied data
	 * @return {@code true}
	 */
	public boolean setData(final ASInputStream stream, FilterFlags flags) {
		this.stream = stream;
		this.flags = flags;
		return true;
	}

	@Override
	public Boolean isStreamKeywordCRLFCompliant() {
		return streamKeywordCRLFCompliant;
	}

	@Override
	public boolean setStreamKeywordCRLFCompliant(boolean streamKeywordCRLFCompliant) {
		this.streamKeywordCRLFCompliant = streamKeywordCRLFCompliant;
		return true;
	}

	@Override
	public Boolean isEndstreamKeywordCRLFCompliant() {
		return endstreamKeywordCRLFCompliant;
	}

	@Override
	public boolean setEndstreamKeywordCRLFCompliant(boolean endstreamKeywordCRLFCompliant) {
		this.endstreamKeywordCRLFCompliant = endstreamKeywordCRLFCompliant;
		return true;
	}

	@Override
	public Long getRealStreamSize() {
		return realStreamSize;
	}

	@Override
	public boolean setRealStreamSize(long realStreamSize) {
		this.realStreamSize = realStreamSize;
		return true;
	}

	/**
	 * Reads the filter chain declared by this stream's {@code /Filter} entry.
	 *
	 * @return the stream's filter names
	 */
	public COSFilters getFilters() {
		return new COSFilters(getKey(ASAtom.FILTER));
	}

	/**
	 * Replaces the declared filter chain and rewrites the stored bytes by
	 * decoding with the current filters and encoding with the supplied filters.
	 *
	 * @param filters replacement filter chain
	 * @throws IOException if the stream cannot be decoded or encoded
	 */
	public void setFilters(final COSFilters filters) throws IOException {
		try (ASInputStream decoded = this.getData(COSStream.FilterFlags.DECODE);
			 SeekableInputStream unfilteredData =
					 SeekableInputStream.getSeekableStream(decoded)) {
			InternalOutputStream fileWithData = InternalOutputStream.getInternalOutputStream();
			setKey(ASAtom.FILTER, filters.getObject());
			ASOutputStream encoder = filters.getOutputStream(fileWithData);
			encoder.write(unfilteredData);
			File encodedDataFile = fileWithData.getFile();
			fileWithData.close();
			this.setData(new InternalInputStream(encodedDataFile, true), FilterFlags.RAW_DATA);
		}
	}

	/**
	 * Returns the processing state associated with the currently stored bytes.
	 *
	 * @return data processing flags
	 */
	public FilterFlags getFilterFlags() {
		return this.flags;
	}

	/**
	 * Sets the processing state associated with the currently stored bytes.
	 *
	 * @param flags data processing flags
	 */
	public void setFilterFlags(final FilterFlags flags) {
		this.flags = flags;
	}

	/**
	 * Returns the stream dictionary's {@code /Length} value.
	 *
	 * @return declared byte length
	 */
	public long getLength() {
		return getIntegerKey(ASAtom.LENGTH);
	}

    public void setLength(final long length) {
		setIntegerKey(ASAtom.LENGTH, length);
	}

	/**
	 * Sets {@code /Length}, retaining an existing indirect reference when the
	 * current length entry is indirect.
	 *
	 * @param length byte length to store
	 */
	public void setIndirectLength(final long length) {
		COSObject obj = getKey(ASAtom.LENGTH);
		obj.setInteger(length);
		if (obj.isIndirect()) {
			obj = COSIndirect.construct(obj);
			setKey(ASAtom.LENGTH, obj);
		}
	}

	/**
	 * Describes which processing has already been applied to stream bytes.
	 */
	public enum FilterFlags {
		RAW_DATA,
		DECODE,
		DECRYPT,
		DECRYPT_AND_DECODE
	}

    private Set<ASAtom> getNonNullKeysExcluding(Set<ASAtom> exclude) {
        Set<ASAtom> keys = new HashSet<>(this.getKeySet());
        keys.removeAll(exclude);
        keys.removeIf(key -> this.getKey(key).getDirectBase() == null);
        return keys;
    }

    @Override
    public boolean isEquivalentTo(COSBase o) {
        if (this == o) return true;
        if (o == null) return false;
        if (o.isIndirect()) {
            return isEquivalentTo(o.getDirectBase());
        }
        if (!(o instanceof COSStream)) return false;
        List<COSBasePair> checkedObjects = new LinkedList<>();
        return isEquivalentTo(o, checkedObjects);
    }

    @Override
    boolean isEquivalentTo(COSBase o, List<COSBasePair> checkedObjects) {
        if (this == o) return true;
        if (o == null) return false;

        if (!(o instanceof COSStream)) return false;
        COSStream that = (COSStream) o;

        if (COSBasePair.listContainsPair(checkedObjects, this, that)) {
            return true;
        }
        COSBasePair.addPairToList(checkedObjects, this, that);

        if (this.stream == null || that.stream == null) {
            return this.stream == that.stream;
        }

        try (ASInputStream thisDecoded = this.getData(FilterFlags.DECODE);
             ASInputStream thatDecoded = that.getData(FilterFlags.DECODE)) {
            if (!equalsDecodedStreams(thisDecoded, thatDecoded)) {
                return false;
            }
        } catch (IOException e) {
            LOGGER.log(Level.FINE, "Exception during comparing decoded streams");
            return false;
        }

        Set<ASAtom> excluded = new HashSet<>(Arrays.asList(ASAtom.LENGTH, ASAtom.FILTER, ASAtom.DECODE_PARMS));
        Set<ASAtom> thisKeys = this.getNonNullKeysExcluding(excluded);
        Set<ASAtom> thatKeys = that.getNonNullKeysExcluding(excluded);

        if (!thisKeys.equals(thatKeys)) {
            return false;
        }

        return isEquivalentKeys(that, thisKeys, checkedObjects);
    }

    private static boolean equalsDecodedStreams(ASInputStream first, ASInputStream second) throws IOException {
        byte[] buf1 = new byte[8192];
        byte[] buf2 = new byte[8192];
        int read1, read2;
        while (true) {
            read1 = first.read(buf1);
            read2 = second.read(buf2);
            if (read1 != read2) return false;
            if (read1 == -1) break;
            for (int i = 0; i < read1; i++) {
                if (buf1[i] != buf2[i]) return false;
            }
        }
        return true;
    }

	@Override
	public boolean equals(Object obj) {
		if (this == obj) {
			return true;
		}
		if (obj == null) {
			return false;
		}
		if (obj instanceof COSObject) {
			return this.equals(((COSObject) obj).get());
		}
		List<COSBasePair> checkedObjects = new LinkedList<>();
		return this.equals(obj, checkedObjects);
	}

	@Override
	boolean equals(Object obj, List<COSBasePair> checkedObjects) {
		if (this == obj) {
			return true;
		}
		if (obj == null) {
			return false;
		}
		if (obj instanceof COSObject) {
			return this.equals(((COSObject) obj).get());
		}
		if (COSBasePair.listContainsPair(checkedObjects, this, (COSBase) obj)) {
			return true;    // Not necessary true, but we should behave as it is
		}
		COSBasePair.addPairToList(checkedObjects, this, (COSBase) obj);
		if (getClass() != obj.getClass()) {
			return false;
		}
		COSStream that = (COSStream) obj;

		for (Map.Entry<ASAtom, COSObject> entry : this.getEntrySet()) {
			if (entry.getKey() == ASAtom.FILTER ||
					entry.getKey() == ASAtom.DECODE_PARMS ||
					entry.getKey() == ASAtom.LENGTH) {
				continue;
			}
			COSBase cosBase = that.getKey(entry.getKey()).get();
			if (!entry.getValue().get().equals(cosBase, checkedObjects)) {
				return false;
			}
		}

		for (Map.Entry<ASAtom, COSObject> entry : that.getEntrySet()) {
			if (entry.getKey() == ASAtom.FILTER || entry.getKey() == ASAtom.DECODE_PARMS ||
					entry.getKey() == ASAtom.LENGTH) {
				continue;
			}
			COSBase cosBase = this.getKey(entry.getKey()).get();
			if (!entry.getValue().get().equals(cosBase, checkedObjects)) {
				return false;
			}
		}

		try {
			if (stream != null ? !equalsStreams(stream, that.stream) : that.stream != null) {
				return false;
			}
		} catch (IOException e) {
			LOGGER.log(Level.FINE, "Exception during comparing streams");
			return false;
		}
		return flags == that.flags;
	}

	private static boolean equalsStreams(ASInputStream first, ASInputStream second) throws IOException {
		first.reset();
		second.reset();
		byte[] tempOne = new byte[1024];
		byte[] tempTwo = new byte[1024];
		int readFromOne;
		int readFromTwo;
		do {
			readFromOne = first.read(tempOne, tempOne.length);
			readFromTwo = second.read(tempTwo, tempTwo.length);
			if (readFromOne != readFromTwo || !Arrays.equals(tempOne, tempTwo)) {
				return false;
			}
		} while (readFromOne != -1);

		return true;
	}

	@Override
	public String toString() {
		return STREAM + '(' + SIZE + " = " + realStreamSize + ')';
	}

}
