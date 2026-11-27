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
import org.verapdf.model.selayer.SEAnnot;
import org.verapdf.pd.structure.PDStructElem;
import org.verapdf.tools.TaggedPDFConstants;

/**
 * Models the standard {@code Annot} structure tag for annotation-related content.
 * <p>In the PDF structure tree (ISO 32000-1/2, clauses 14.7-14.8), this semantic role associates the tagged
 * content with an annotation; PDF/UA rules use the typed node to validate that relationship.
 * {@link GFSEFactory} creates this adapter while the validation model exposes it to profile rules.</p>
 * <p><b>.NET porting notes:</b> port as {@code VeraPdf.Validation.Gf.Model.Impl.Pd.Gfse.GFSEAnnot},
 * implementing {@code VeraPdf.Model.SELayer.ISEAnnot}. Preserve the standard-type and model-type strings
 * passed to {@code GFPDStructElem}; they are rule-facing identifiers, not C# naming choices.</p>
 */
public class GFSEAnnot extends GFPDStructElem implements SEAnnot {

    public static final String ANNOT_STRUCTURE_ELEMENT_TYPE = "SEAnnot";

    public GFSEAnnot(PDStructElem structElemDictionary) {
        super(structElemDictionary, TaggedPDFConstants.ANNOT, ANNOT_STRUCTURE_ELEMENT_TYPE);
    }
}
