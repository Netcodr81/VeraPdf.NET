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
package org.verapdf.cos.xref;

/**
 * Entry in a PDF cross-reference section, recording an object's offset,
 * generation, and whether the object number is free or in use.
 *
 *
 * <h2>.NET porting notes</h2>
 * In C#, represent {@code COSXRefEntry} with immutable records containing numeric
 * object/generation identifiers and source offsets. Keep the xref table sparse (for example,
 * {@code IReadOnlyDictionary<COSKey, COSXRefEntry>}) and provide deterministic range/section
 * validation before resolving objects.
 *
 * @author Timur Kamalov
 */
public class COSXRefEntry {

	public static final COSXRefEntry FIRST_XREF_ENTRY = new COSXRefEntry(0, 65535, 'f');

	/**
	 * Is offset of object in the document.
	 */
	public long offset;
	/**
	 * Is generation number for this entry.
	 */
	public int generation;
	/**
	 * Is 'f' is entry is free and 'n' otherwise.
	 */
	public char free;

	/**
	 * Default constructor that initializes offset and generation with zeroes,
	 * entry is marked as in-use.
	 */
	public COSXRefEntry() {
		this(0, 0, 'n');
	}

	/**
	 * Constructor from offset and generation. Entry is marked as in-use.
	 *
	 * @param offset is offset of object represented by this entry.
	 * @param generation is generation of object represented by this entry.
	 */
	public COSXRefEntry(long offset, int generation) {
		this(offset, generation, 'n');
	}

	/**
	 * Constructor setting offset, generation and free value.
	 *
	 * @param offset is offset of object represented by this entry.
	 * @param generation is generation of object represented by this entry.
	 * @param free is 'f' is entry is free and 'n' otherwise.
	 */
	public COSXRefEntry(long offset, int generation, char free) {
		this.offset = offset;
		this.generation = generation;
		this.free = free;
	}

	@Override
	public boolean equals(Object o) {
		if (this == o) {
			return true;
		}
		if (o == null || getClass() != o.getClass()) {
			return false;
		}
		COSXRefEntry entry = (COSXRefEntry) o;
		return offset == entry.offset && generation == entry.generation && free == entry.free;
	}
}
