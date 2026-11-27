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
import org.verapdf.model.selayer.SEArt;
import org.verapdf.pd.structure.PDStructElem;
import org.verapdf.tools.TaggedPDFConstants;

/**
 * Represents an {@code Art} structure element, a large-scale division of a document's content.
 * <p>ISO 32000's structure tree lets consumers interpret this semantic grouping independently of page layout;
 * veraPDF creates this thin typed adapter through {@link GFSEFactory} so PDF/UA profiles can identify it.</p>
 * <p><b>.NET porting notes:</b> use {@code VeraPdf.Validation.Gf.Model.Impl.Pd.Gfse.GFSEArt} and
 * {@code VeraPdf.Model.SELayer.ISEArt}; retain the exact PDF tag and model type values from this constructor.</p>
 */
public class GFSEArt extends GFPDStructElem implements SEArt {

    public static final String ART_STRUCTURE_ELEMENT_TYPE = "SEArt";

    public GFSEArt(PDStructElem structElemDictionary) {
        super(structElemDictionary, TaggedPDFConstants.ART, ART_STRUCTURE_ELEMENT_TYPE);
    }
}
