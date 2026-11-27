/*
 * This file is part of veraPDF WCAG Validation, a module of the veraPDF project.
 * Copyright (c) 2015-2026, veraPDF Consortium <info@verapdf.org>
 * All rights reserved.
 *
 * veraPDF WCAG Validation is free software: you can redistribute it and/or modify
 * it under the terms of either:
 *
 * The GNU General public license GPLv3+.
 * You should have received a copy of the GNU General Public License
 * along with veraPDF WCAG Validation as the LICENSE.GPL file in the root of the source
 * tree.  If not, see http://www.gnu.org/licenses/ or
 * https://www.gnu.org/licenses/gpl-3.0.en.html.
 *
 * The Mozilla Public License MPLv2+.
 * You should have received a copy of the Mozilla Public License along with
 * veraPDF WCAG Validation as the LICENSE.MPL file in the root of the source tree.
 * If a copy of the MPL was not distributed with this file, you can obtain one at
 * http://mozilla.org/MPL/2.0/.
 */
package org.verapdf.gf.model.impl.sa;

import org.verapdf.pd.structure.PDStructElem;
import org.verapdf.tools.AttributeHelper;
import org.verapdf.wcag.algorithms.entities.IAttributesDictionary;


/**
 * Supplies table-cell span attributes from a tagged structure element to the algorithms layer.
 *
 * <p>PDF table cells can declare {@code ColSpan} and {@code RowSpan} in their attribute
 * dictionaries. The semantic analyzer asks this adapter for those values while building table
 * relationships, rather than reading raw COS attributes itself.</p>
 *
 * <p><b>.NET porting notes:</b> Implement the algorithms contract as
 * {@code VeraPdf.Validation.Wcag.Model.Impl.Sa.GFSAAttributesDictionary} and retain the adapter
 * boundary to {@code VeraPdf.Parser.Pd.PdfStructure}. The WCAG {@code IAttributesDictionary}
 * contract has no .NET counterpart yet; preserve the helper's default-span semantics.</p>
 *
 * @author Maxim Plushchov
 */
public class GFSAAttributesDictionary implements IAttributesDictionary {

	private final PDStructElem pdStructElem;

	/** Binds the adapter to the tagged table element whose attributes are queried. */
	public GFSAAttributesDictionary(PDStructElem pdStructElem) {
		this.pdStructElem = pdStructElem;
	}

	@Override
	/** Returns the normalized column span, including the PDF default when absent. */
	public long getColSpan() {
		return AttributeHelper.getColSpan(pdStructElem.getObject());
	}

	@Override
	/** Returns the normalized row span, including the PDF default when absent. */
	public long getRowSpan() {
		return AttributeHelper.getRowSpan(pdStructElem.getObject());
	}
}
