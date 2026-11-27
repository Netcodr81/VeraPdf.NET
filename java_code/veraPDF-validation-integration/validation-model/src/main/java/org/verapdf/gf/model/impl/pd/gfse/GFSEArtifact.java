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
import org.verapdf.model.selayer.SEArtifact;
import org.verapdf.pd.structure.PDStructElem;
import org.verapdf.tools.TaggedPDFConstants;

/**
 * Represents an {@code Artifact}, content explicitly marked as non-structural decoration or pagination furniture.
 * <p>Artifacts are outside the logical reading order, unlike ordinary structure elements, and must not be
 * mistaken for meaningful tagged content by PDF/UA checks. The factory supplies this typed node to the model.</p>
 * <p><b>.NET porting notes:</b> implement {@code VeraPdf.Model.SELayer.ISEArtifact} on
 * {@code VeraPdf.Validation.Gf.Model.Impl.Pd.Gfse.GFSEArtifact}; preserve the {@code Artifact} tag identity.</p>
 */
public class GFSEArtifact extends GFPDStructElem implements SEArtifact {

    public static final String ARTIFACT_STRUCTURE_ELEMENT_TYPE = "SEArtifact";

    public GFSEArtifact(PDStructElem structElemDictionary) {
        super(structElemDictionary, TaggedPDFConstants.ARTIFACT, ARTIFACT_STRUCTURE_ELEMENT_TYPE);
    }
}
