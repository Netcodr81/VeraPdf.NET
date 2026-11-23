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
package org.verapdf.cos.visitor;

import org.verapdf.cos.*;

/**
 * Side-effecting visitor for COS values. Each method corresponds to a concrete
 * COS type, including the indirect-reference node.
 *
 * <h2>.NET porting notes</h2>
 * In C#, {@code IVisitor} can remain a visitor interface for maximal parity, or be replaced
 * internally by exhaustive pattern matching over a sealed COS hierarchy. If both APIs are kept,
 * make the visitor accept a nullable result and document which indirect-reference nodes are
 * traversed.
 */
public interface IVisitor {

	void visitFromBoolean(final COSBoolean obj);
	void visitFromInteger(final COSInteger obj);
	void visitFromReal(final COSReal obj);
	void visitFromString(final COSString obj);
	void visitFromName(final COSName obj);
	void visitFromArray(final COSArray obj);
	void visitFromDictionary(final COSDictionary obj);
	void visitFromStream(final COSStream obj);
	void visitFromNull(final COSNull obj);
	void visitFromIndirect(final COSIndirect obj);

}
