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
import org.verapdf.model.selayer.SELink;
import org.verapdf.pd.structure.PDStructElem;
import org.verapdf.tools.TaggedPDFConstants;

/**
 * Models a {@code Link} structure element, grouping linked text or graphics in the logical reading order.
 * <p>The structure role can be associated with annotation objects via PDF object-reference children; profiles
 * use this typed wrapper alongside the page-level annotation model to check accessible link semantics.</p>
 * <p><b>.NET porting notes:</b> implement {@code VeraPdf.Model.SELayer.ISELink} on
 * {@code VeraPdf.Validation.Gf.Model.Impl.Pd.Gfse.GFSELink}; use the parser's PDF structure and annotation adapters.</p>
 */
public class GFSELink extends GFPDStructElem implements SELink {

    public static final String LINK_STRUCTURE_ELEMENT_TYPE = "SELink";

    public GFSELink(PDStructElem structElemDictionary) {
        super(structElemDictionary, TaggedPDFConstants.LINK, LINK_STRUCTURE_ELEMENT_TYPE);
    }
}
