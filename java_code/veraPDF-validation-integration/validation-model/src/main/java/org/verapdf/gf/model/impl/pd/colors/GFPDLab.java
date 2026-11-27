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
package org.verapdf.gf.model.impl.pd.colors;

import org.verapdf.model.pdlayer.PDLab;

/**
 * Adapts the Lab color space, which represents color using lightness and opponent axes.
 *
 * <p>Lab is a CIE-based three-component space with a lightness value and two opponent
 * color axes, interpreted against a white point and ranges. It is device-independent
 * and exposed as a distinct PD model type for color-management validation.</p>
 *
 * <p><b>.NET porting notes:</b> Suggested type:
 * {@code VeraPdf.Validation.Gf.Model.Impl.Pd.Colors.GFPDLab}, implementing
 * {@code VeraPdf.Model.PDLayer.IPDLab}; preserve the common component-count adapter
 * and delegate PDF array parsing to the parser layer.</p>
 *
 * @author Maksim Bezrukov
 */
public class GFPDLab extends GFPDColorSpace implements PDLab {

    /** Model type name for Lab color spaces. */
    public static final String LAB_COLOR_SPACE_TYPE = "PDLab";

    /** Creates the model wrapper for the parser's Lab space. */
    public GFPDLab(
            org.verapdf.pd.colors.PDLab simplePDObject) {
        super(simplePDObject, LAB_COLOR_SPACE_TYPE);
    }
}
