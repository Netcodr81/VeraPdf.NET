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
import org.verapdf.model.selayer.SELbl;
import org.verapdf.pd.structure.PDStructElem;
import org.verapdf.tools.TaggedPDFConstants;

/**
 * Represents an {@code Lbl}, the label or marker associated with a list item.
 * <p>Separating the label from {@code LBody} allows readers to announce list numbering or bullets without
 * confusing them with the item's text; its parent and siblings remain available through the base adapter.</p>
 * <p><b>.NET porting notes:</b> use {@code VeraPdf.Validation.Gf.Model.Impl.Pd.Gfse.GFSELbl},
 * implementing {@code VeraPdf.Model.SELayer.ISELbl} and retaining the exact {@code Lbl} role name.</p>
 */
public class GFSELbl extends GFPDStructElem implements SELbl {

    public static final String LBL_STRUCTURE_ELEMENT_TYPE = "SELbl";

    public GFSELbl(PDStructElem structElemDictionary) {
        super(structElemDictionary, TaggedPDFConstants.LBL, LBL_STRUCTURE_ELEMENT_TYPE);
    }
}
