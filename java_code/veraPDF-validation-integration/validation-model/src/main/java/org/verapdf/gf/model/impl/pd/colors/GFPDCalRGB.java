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

import org.verapdf.model.pdlayer.PDCalRGB;

/**
 * Adapts the calibrated RGB color space defined by CIE white-point and matrix data.
 *
 * <p>{@code CalRGB} has three components but, unlike device RGB, defines how those
 * components map to a calibrated CIE space. It is exposed as its own PD model type so
 * profiles can inspect its parameters and ensure color spaces meet PDF/A requirements.</p>
 *
 * <p><b>.NET porting notes:</b> Suggested type:
 * {@code VeraPdf.Validation.Gf.Model.Impl.Pd.Colors.GFPDCalRGB}, implementing
 * {@code VeraPdf.Model.PDLayer.IPDCalRGB}; delegate low-level values to the parser-side
 * color-space abstraction and reuse the base component count.</p>
 *
 * @author Maksim Bezrukov
 */
public class GFPDCalRGB extends GFPDColorSpace implements PDCalRGB {

    /** Model type name for calibrated RGB resources. */
    public static final String CAL_RGB_TYPE = "PDCalRGB";

    /** Creates the model wrapper for the parser's calibrated RGB space. */
    public GFPDCalRGB(
            org.verapdf.pd.colors.PDCalRGB simplePDObject) {
        super(simplePDObject, CAL_RGB_TYPE);
    }
}
