/*
 * This file is part of veraPDF Validation, a module of the veraPDF project.
 * Copyright (c) 2015-2026, veraPDF Consortium <info@verapdf.org>
 * All rights reserved.
 *
 * veraPDF Validation is free software: you can redistribute it and/or modify
 * it under the terms of either:
 *
 * The GNU General public license GPLv3+.
 * You should have received a copy of the GNU General Public License
 * along with veraPDF Validation as the LICENSE.GPL file in the root of the source
 * tree.  If not, see http://www.gnu.org/licenses/ or
 * https://www.gnu.org/licenses/gpl-3.0.en.html.
 *
 * The Mozilla Public License MPLv2+.
 * You should have received a copy of the Mozilla Public License along with
 * veraPDF Validation as the LICENSE.MPL file in the root of the source tree.
 * If a copy of the MPL was not distributed with this file, you can obtain one at
 * http://mozilla.org/MPL/2.0/.
 */
package org.verapdf.gf.model.impl.pd.gfse;

import org.verapdf.gf.model.impl.pd.GFPDStructElem;
import org.verapdf.model.selayer.SEBlockQuote;
import org.verapdf.pd.structure.PDStructElem;
import org.verapdf.tools.TaggedPDFConstants;

/**
 * Models a {@code BlockQuote}, a block-level quotation in the logical document hierarchy.
 * <p>The role distinguishes quoted passages from surrounding prose for assistive-technology reading and lets
 * PDF/UA validation inspect the correct semantic type rather than infer it from visual formatting.</p>
 * <p><b>.NET porting notes:</b> port to {@code VeraPdf.Validation.Gf.Model.Impl.Pd.Gfse.GFSEBlockQuote},
 * implementing {@code VeraPdf.Model.SELayer.ISEBlockQuote}; keep the PDF tag spelling unchanged.</p>
 */
public class GFSEBlockQuote extends GFPDStructElem implements SEBlockQuote {

    public static final String BLOCK_QUOTE_STRUCTURE_ELEMENT_TYPE = "SEBlockQuote";

    public GFSEBlockQuote(PDStructElem structElemDictionary) {
        super(structElemDictionary, TaggedPDFConstants.BLOCK_QUOTE, BLOCK_QUOTE_STRUCTURE_ELEMENT_TYPE);
    }
}
