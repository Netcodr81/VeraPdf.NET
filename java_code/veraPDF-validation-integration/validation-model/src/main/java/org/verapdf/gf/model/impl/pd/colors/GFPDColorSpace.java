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

import org.verapdf.gf.model.impl.pd.GFPDResource;
import org.verapdf.model.pdlayer.PDColorSpace;

/**
 * Common PD-model adapter for PDF color spaces and their component count.
 *
 * <p>A color space defines the interpretation of numeric color operands in content
 * streams, image samples, and patterns. This base wraps parser-side color-space objects
 * while subtypes expose PDF-specific links such as an ICC profile or alternate space.
 * The component count is a DSL scalar consumed by validation profiles.</p>
 *
 * <p><b>.NET porting notes:</b> Suggested base:
 * {@code VeraPdf.Validation.Gf.Model.Impl.Pd.Colors.GFPDColorSpace}, implementing
 * {@code VeraPdf.Model.PDLayer.IPDColorSpace}. The model contract maps the nullable DSL
 * integer to {@code long?}; use {@code VeraPdf.Parser} color-space types when ported.</p>
 *
 * @author Maksim Bezrukov
 */
public class GFPDColorSpace extends GFPDResource implements PDColorSpace {

    /** Initializes a color-space adapter with its profile-visible model type name. */
    protected GFPDColorSpace(org.verapdf.pd.colors.PDColorSpace simplePDObject, String type) {
        super(simplePDObject, type);
    }

    @Override
    /** Returns the number of input components, widened for the model's integer property. */
    public Long getnrComponents() {
        return (long) ((org.verapdf.pd.colors.PDColorSpace) simplePDObject).getNumberOfComponents();
    }
}
