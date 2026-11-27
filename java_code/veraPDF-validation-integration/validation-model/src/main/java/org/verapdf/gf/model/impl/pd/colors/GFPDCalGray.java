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

import org.verapdf.model.pdlayer.PDCalGray;

/**
 * Adapts the calibrated grayscale color space, whose gray value is interpreted using white-point data.
 *
 * <p>Unlike device gray, {@code CalGray} is a CIE-based space with a calibrated white
 * point and gamma. It has one component and is validated as a distinct color-space
 * subtype. The color-space factory creates this model adapter while traversing PD
 * resources used by page content, images, and forms.</p>
 *
 * <p><b>.NET porting notes:</b> Suggested type:
 * {@code VeraPdf.Validation.Gf.Model.Impl.Pd.Colors.GFPDCalGray}, implementing
 * {@code VeraPdf.Model.PDLayer.IPDCalGray}; use the parser's color-space model and
 * preserve the shared {@code GFPDColorSpace} component-count behavior.</p>
 *
 * @author Maksim Bezrukov
 */
public class GFPDCalGray extends GFPDColorSpace implements PDCalGray {

    /** Model type name for calibrated grayscale resources. */
    public static final String CAL_GRAY_TYPE = "PDCalGray";

    /** Creates the model wrapper for the parser's calibrated gray space. */
    public GFPDCalGray(
            org.verapdf.pd.colors.PDCalGray simplePDObject) {
        super(simplePDObject, CAL_GRAY_TYPE);
    }
}
