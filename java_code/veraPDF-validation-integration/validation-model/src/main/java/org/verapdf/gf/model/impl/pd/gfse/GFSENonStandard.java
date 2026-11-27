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
import org.verapdf.model.selayer.SENonStandard;
import org.verapdf.pd.structure.PDStructElem;

import java.util.Objects;

/**
 * Represents a structure tag that is not one of the directly recognized standard types.
 * <p>The constructor retains the resolved standard-type string, which can be {@code null};
 * {@link #getisNotMappedToStandardType()} reports true only when no remapped standard role exists and the role
 * map is not circular. This distinguishes a custom tag with a usable role-map mapping from an unmapped tag.</p>
 * <p><b>.NET porting notes:</b> port as {@code VeraPdf.Validation.Gf.Model.Impl.Pd.Gfse.GFSENonStandard},
 * implementing {@code VeraPdf.Model.SELayer.ISENonStandard}; preserve nullable string and nullable boolean semantics.</p>
 */
public class GFSENonStandard extends GFPDStructElem implements SENonStandard {

    public static final String NON_STANDARD_STRUCTURE_ELEMENT_TYPE = "SENonStandard";

    public GFSENonStandard(PDStructElem structElemDictionary, String standardType) {
        super(structElemDictionary, standardType, NON_STANDARD_STRUCTURE_ELEMENT_TYPE);
    }

    @Override
    public Boolean getisNotMappedToStandardType() {
        return getremappedStandardType() == null && !Objects.equals(getcircularMappingExist(), true);
    }
}
