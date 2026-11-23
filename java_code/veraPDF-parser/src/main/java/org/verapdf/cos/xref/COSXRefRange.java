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
 * A contiguous range of object numbers in a PDF cross-reference stream/table;
 * {@code count} is the number of entries beginning at {@code start}.
 *
 * <h2>.NET porting notes</h2>
 * In C#, represent {@code COSXRefRange} with immutable records containing numeric
 * object/generation identifiers and source offsets. Keep the xref table sparse (for example,
 * {@code IReadOnlyDictionary<COSKey, COSXRefEntry>}) and provide deterministic range/section
 * validation before resolving objects.
 */
public class COSXRefRange {

	public int start;
	public int count;

	public COSXRefRange() {
	}

	public COSXRefRange(final int start) {
		this(start, 1);
	}

	public COSXRefRange(final int start, final int count) {
		this.start = start;
		this.count = count;
	}

	/**
	 * Returns the first object number after this range.
	 *
	 * @return {@code start + count}
	 */
	public int next() {
		return this.start + this.count;
	}

}
