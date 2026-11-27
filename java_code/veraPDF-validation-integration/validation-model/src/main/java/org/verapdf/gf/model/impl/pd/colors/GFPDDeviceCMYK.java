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

import org.verapdf.model.pdlayer.PDDeviceCMYK;

/**
 * Provides the process-color CMYK space, including its inherited-resource variant.
 *
 * <p>DeviceCMYK represents cyan, magenta, yellow, and black directly and has four
 * components. The two singleton wrappers distinguish an explicitly selected color
 * space from the inherited resource form used by parser/model logic; both are used by
 * color-space resolution and can be inspected by PDF/A rules.</p>
 *
 * <p><b>.NET porting notes:</b> Suggested type:
 * {@code VeraPdf.Validation.Gf.Model.Impl.Pd.Colors.GFPDDeviceCMYK}, implementing
 * {@code VeraPdf.Model.PDLayer.IPDDeviceCMYK}. Translate the two static instances to
 * {@code static readonly} fields or {@code Lazy<T>}; preserve their distinct identity.</p>
 *
 * @author Maksim Bezrukov
 */
public class GFPDDeviceCMYK extends GFPDColorSpace implements PDDeviceCMYK {

    /** Canonical explicit DeviceCMYK model instance. */
    private static final PDDeviceCMYK INSTANCE = new GFPDDeviceCMYK(
            org.verapdf.pd.colors.PDDeviceCMYK.INSTANCE);
    /** Canonical inherited DeviceCMYK model instance. */
    private static final PDDeviceCMYK INHERITED_INSTANCE = new GFPDDeviceCMYK(
            org.verapdf.pd.colors.PDDeviceCMYK.INHERITED_INSTANCE);

    /** Model type name for the CMYK device color space. */
    public static final String DEVICE_CMYK_TYPE = "PDDeviceCMYK";

    /** Creates the private singleton wrappers around parser constants. */
    private GFPDDeviceCMYK(
            org.verapdf.pd.colors.PDDeviceCMYK simplePDObject) {
        super(simplePDObject, DEVICE_CMYK_TYPE);
    }

    /** Returns the explicit DeviceCMYK wrapper. */
    public static PDDeviceCMYK getInstance() {
        return INSTANCE;
    }

    /** Returns the inherited DeviceCMYK wrapper. */
    public static PDDeviceCMYK getInheritedInstance() {
        return INHERITED_INSTANCE;
    }
}
