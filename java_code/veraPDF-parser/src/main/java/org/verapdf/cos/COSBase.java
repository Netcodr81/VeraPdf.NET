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
import org.verapdf.as.io.ASInputStream;
import org.verapdf.cos.visitor.ICOSVisitor;
import org.verapdf.cos.visitor.IVisitor;

import java.util.Collection;
import java.util.List;
import java.util.Set;

/**
 * Common interface for PDF COS values, including direct values and indirect
 * references. Concrete values provide type-specific access and visitor dispatch.
 *
 * <h2>.NET porting notes</h2>
 * In C#, {@code COSBase} belongs in a closed COS object hierarchy with exhaustive pattern
 * matching. Prefer interfaces or abstract sealed records for shared behavior, and make
 * indirect-reference resolution explicit so direct values, PDF null, and unresolved references
 * cannot be conflated.
 */
public abstract class COSBase {

	private COSKey indirectKey;

	public COSBase() {
	}

	/**
	 * Returns the concrete COS kind represented by this value.
	 *
	 * @return COS value type
	 */
	public abstract COSObjType getType();

	/**
	 * Returns the object key associated with this value, if it represents or
	 * belongs to an indirect object.
	 *
	 * @return the indirect object key, or {@code null} when none is associated
	 */
	public COSKey getObjectKey() {
		return this.indirectKey;
	}

	/**
	 * Associates an indirect object key with this value.
	 *
	 * @param indirectKey key to associate, or {@code null} to clear it
	 */
	public void setObjectKey(COSKey indirectKey) {
		this.indirectKey = indirectKey;
	}

	// VISITOR DESIGN PATTERN
	public abstract void accept(final IVisitor visitor);
	public abstract Object accept(final ICOSVisitor visitor);

	// BOOLEAN VALUES
	public abstract Boolean getBoolean();
	public abstract boolean setBoolean(final boolean value);

	// INTEGER NUMBERS
	public abstract Long getInteger();
	public abstract boolean setInteger(final long value);

	// REAL NUMBERS
	public abstract Double getReal();
	public abstract boolean setReal(final double value);

	// STRINGS
	public abstract String getString();
	public abstract boolean setString(final String value);
	public abstract boolean setString(final String value, final boolean isHex);

	// NAMES
	public abstract ASAtom getName();
	public abstract boolean setName(final ASAtom value);

	// NUMBERS OF ELEMENTS FOR ARRAY AND DICTIONARY
	public abstract Integer size();

	// ARRAYS
	public abstract COSObject at(final int i);
	public abstract boolean add(final COSObject value);
	public abstract boolean set(final int i, final COSObject value);
	public abstract boolean insert(final int i, final COSObject value);
	public abstract void remove(final int i);
	public abstract boolean setArray();
	public abstract boolean setArray(final int size, final COSObject[] value);
	public abstract boolean setArray(final int size, final double[] value);
	public abstract void clearArray();

	// DICTIONARIES
	public abstract Boolean knownKey(final ASAtom key);
	public abstract COSObject getKey(final ASAtom key);
	public abstract boolean setKey(final ASAtom key, final COSObject value);
	public abstract Boolean getBooleanKey(final ASAtom key);
	public abstract boolean setBooleanKey(final ASAtom key, final boolean value);
	public abstract Long getIntegerKey(final ASAtom key);
	public abstract boolean setIntegerKey(final ASAtom key, final long value);
	public abstract Double getRealKey(final ASAtom key);
	public abstract boolean setRealKey(final ASAtom key, final double value);
	public abstract String getStringKey(final ASAtom key);
	public abstract boolean setStringKey(final ASAtom key, final String value);
	public abstract ASAtom getNameKey(final ASAtom key);
	public abstract String getNameKeyStringValue(final ASAtom key);
	public abstract String getNameKeyUnicodeValue(final ASAtom key);
	public abstract boolean setNameKey(final ASAtom key, final ASAtom value);
	public abstract boolean setArrayKey(final ASAtom key);
	public abstract boolean setArrayKey(final ASAtom key, final COSObject array);
	public abstract boolean setArrayKey(final ASAtom key, final int size, final COSObject[] value);
	public abstract boolean setArrayKey(final ASAtom key, final int size, final double[] value);
	public abstract void removeKey(final ASAtom key);
	public abstract Set<ASAtom> getKeySet();
	public abstract Collection<COSObject> getValues();

	// STREAMS
	public abstract ASInputStream getData();
	public abstract ASInputStream getData(final COSStream.FilterFlags flags);

	public abstract boolean setData(final ASInputStream stream);
	public abstract boolean setData(final ASInputStream stream, final COSStream.FilterFlags flags);

	public abstract Boolean isStreamKeywordCRLFCompliant();
	public abstract boolean setStreamKeywordCRLFCompliant(final boolean streamKeywordCRLFCompliant);

	public abstract Long getRealStreamSize();
	public abstract boolean setRealStreamSize(final long realStreamSize);

	public abstract Boolean isEndstreamKeywordCRLFCompliant();
	public abstract boolean setEndstreamKeywordCRLFCompliant(final boolean endstreamKeywordCRLFCompliant);

	// INDIRECT OBJECT
	/**
	 * Indicates whether this value is an indirect reference.
	 *
	 * @return {@code true} for an indirect reference and {@code false} for a
	 * direct value
	 */
	public abstract Boolean isIndirect();
	public abstract COSKey getKey();
	public abstract COSDocument getDocument();
	public abstract boolean setKey(final COSKey key, final COSDocument document);
	/**
	 * Resolves this value to its direct-object wrapper when applicable.
	 *
	 * @return the direct wrapper, or {@code null} when it cannot be resolved
	 */
	public abstract COSObject getDirect();
	/**
	 * Resolves this value to its underlying direct COS value.
	 *
	 * @return the direct COS value, or {@code null} when unresolved
	 */
	public abstract COSBase getDirectBase();
	public abstract boolean setDirect(final COSObject value);

	public abstract void mark();

    /**
     * Compares COS values by their semantic contents, resolving indirect
     * references as needed.
     *
     * @param obj value to compare
     * @return {@code true} when the values are equivalent
     */
    public abstract boolean isEquivalentTo(COSBase obj);

    boolean isEquivalentTo(COSBase o, List<COSBasePair> checkedObjects) {
        return isEquivalentTo(o);
    }

	boolean equals(Object obj, List<COSBasePair> checkedObjects) {
		return this.equals(obj);
	}
}
