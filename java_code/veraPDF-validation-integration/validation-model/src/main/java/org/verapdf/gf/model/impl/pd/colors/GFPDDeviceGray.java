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

import org.verapdf.model.pdlayer.PDDeviceGray;

/**
 * Provides the one-component DeviceGray space and its inherited-resource variant.
 *
 * <p>DeviceGray encodes a gray value directly without calibration data. The separate
 * inherited singleton indicates that the space came from resource inheritance rather
 * than an explicit selection, which can matter to rule evaluation.</p>
 *
 * <p><b>.NET porting notes:</b> Suggested type:
 * {@code VeraPdf.Validation.Gf.Model.Impl.Pd.Colors.GFPDDeviceGray}, implementing
 * {@code VeraPdf.Model.PDLayer.IPDDeviceGray}. Map the static wrappers to
 * {@code static readonly} instances and preserve explicit versus inherited state.</p>
 *
 * @author Maksim Bezrukov
 */
public class GFPDDeviceGray extends GFPDColorSpace implements PDDeviceGray {

    /** Canonical explicit DeviceGray model instance. */
    private static final PDDeviceGray INSTANCE = new GFPDDeviceGray(
            org.verapdf.pd.colors.PDDeviceGray.INSTANCE);
    /** Canonical inherited DeviceGray model instance. */
    private static final PDDeviceGray INHERITED_INSTANCE = new GFPDDeviceGray(
            org.verapdf.pd.colors.PDDeviceGray.INHERITED_INSTANCE);

    /** Model type name for the gray device color space. */
    public static final String DEVICE_GRAY_TYPE = "PDDeviceGray";

    /** Creates the private singleton wrappers around parser constants. */
    private GFPDDeviceGray(
            org.verapdf.pd.colors.PDDeviceGray simplePDObject) {
        super(simplePDObject, DEVICE_GRAY_TYPE);
    }

    /** Returns the explicit DeviceGray wrapper. */
    public static PDDeviceGray getInstance() {
        return INSTANCE;
    }

    /** Returns the inherited DeviceGray wrapper. */
    public static PDDeviceGray getInheritedInstance() {
        return INHERITED_INSTANCE;
    }
}
