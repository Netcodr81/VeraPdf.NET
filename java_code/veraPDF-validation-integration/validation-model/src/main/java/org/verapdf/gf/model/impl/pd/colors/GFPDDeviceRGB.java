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

import org.verapdf.model.pdlayer.PDDeviceRGB;

/**
 * Provides the three-component DeviceRGB space and its inherited-resource variant.
 *
 * <p>DeviceRGB expresses red, green, and blue values directly and depends on the output
 * device rather than an embedded profile. Distinct singleton wrappers identify explicit
 * and inherited resource resolution for validation.</p>
 *
 * <p><b>.NET porting notes:</b> Suggested type:
 * {@code VeraPdf.Validation.Gf.Model.Impl.Pd.Colors.GFPDDeviceRGB}, implementing
 * {@code VeraPdf.Model.PDLayer.IPDDeviceRGB}. Translate singleton fields to
 * {@code static readonly}; preserve the separate inherited instance.</p>
 *
 * @author Maksim Bezrukov
 */
public class GFPDDeviceRGB extends GFPDColorSpace implements PDDeviceRGB {
    /** Canonical explicit DeviceRGB model instance. */
    private static final PDDeviceRGB INSTANCE = new GFPDDeviceRGB(
            org.verapdf.pd.colors.PDDeviceRGB.INSTANCE);
    /** Canonical inherited DeviceRGB model instance. */
    private static final PDDeviceRGB INHERITED_INSTANCE = new GFPDDeviceRGB(
            org.verapdf.pd.colors.PDDeviceRGB.INHERITED_INSTANCE);

    /** Model type name for the RGB device color space. */
    public static final String DEVICE_RGB_TYPE = "PDDeviceRGB";

    /** Creates the private singleton wrappers around parser constants. */
    private GFPDDeviceRGB(
            org.verapdf.pd.colors.PDDeviceRGB simplePDObject) {
        super(simplePDObject, DEVICE_RGB_TYPE);
    }

    /** Returns the explicit DeviceRGB wrapper. */
    public static PDDeviceRGB getInstance() {
        return INSTANCE;
    }

    /** Returns the inherited DeviceRGB wrapper. */
    public static PDDeviceRGB getInheritedInstance() {
        return INHERITED_INSTANCE;
    }
}
