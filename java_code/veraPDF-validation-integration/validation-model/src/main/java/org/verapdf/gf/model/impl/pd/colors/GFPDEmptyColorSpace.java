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

import org.verapdf.model.baselayer.Object;

import java.util.List;

import static org.verapdf.gf.model.impl.pd.colors.GFPDDeviceGray.DEVICE_GRAY_TYPE;

/**
 * Sentinel color-space adapter used when no concrete color-space object is available.
 *
 * <p>The adapter deliberately reports a null component count and has no additional
 * links, allowing callers to retain a model object for a missing or unresolved color
 * space without fabricating valid PDF data. It currently uses the DeviceGray model type
 * identifier for compatibility with the existing validation contract.</p>
 *
 * <p><b>.NET porting notes:</b> Suggested type:
 * {@code VeraPdf.Validation.Gf.Model.Impl.Pd.Colors.GFPDEmptyColorSpace}. Preserve the
 * nullable {@code long?} component count and avoid replacing this sentinel with a valid
 * DeviceGray singleton; an unresolved space is semantically different from gray.</p>
 *
 * @author Sergey Shemyakov
 */
public class GFPDEmptyColorSpace extends GFPDColorSpace {

    /** Creates a sentinel with no source color-space object. */
    public GFPDEmptyColorSpace() {
        super(null, DEVICE_GRAY_TYPE);
    }

    @Override
    /** This sentinel has no color-space-specific links. */
    public List<? extends Object> getLinkedObjects(String link) {
        return super.getLinkedObjects(link);
    }

    @Override
    /** Returns null because the source color space and component count are unavailable. */
    public Long getnrComponents() {
        return null;
    }
}
