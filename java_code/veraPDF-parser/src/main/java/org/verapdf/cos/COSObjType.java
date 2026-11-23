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

/**
 * The COS value kinds represented by this parser, including an undefined kind
 * for empty object wrappers.
 *
 * <h2>.NET porting notes</h2>
 * In C#, {@code COSObjType} belongs in a closed COS object hierarchy with exhaustive pattern
 * matching. Prefer interfaces or abstract sealed records for shared behavior, and make
 * indirect-reference resolution explicit so direct values, PDF null, and unresolved references
 * cannot be conflated.
 */
public enum COSObjType {

	COS_UNDEFINED,
	COS_NULL,
	COS_INTEGER,
	COS_REAL,
	COS_BOOLEAN,
	COS_NAME,
	COS_STRING,
	COS_DICT,
	COS_ARRAY,
	COS_STREAM;

	public boolean isNumber() {
		return this == COS_INTEGER || this == COS_REAL;
	}

	public boolean isBoolean() {
		return this == COS_BOOLEAN;
	}

	public boolean isDictionaryBased() {
		return this == COS_DICT || this == COS_STREAM;
	}
}
