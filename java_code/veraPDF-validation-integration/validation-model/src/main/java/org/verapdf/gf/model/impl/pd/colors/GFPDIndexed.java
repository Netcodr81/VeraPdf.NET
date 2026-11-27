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

import org.verapdf.gf.model.factory.colors.ColorSpaceFactory;
import org.verapdf.model.baselayer.Object;
import org.verapdf.model.pdlayer.PDColorSpace;
import org.verapdf.model.pdlayer.PDIndexed;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

/**
 * Adapts an Indexed color space, where each sample indexes a lookup table in a base space.
 *
 * <p>Indexed spaces store compact integer indices rather than full color components.
 * The index is expanded through a lookup table and interpreted using the one-component
 * or multi-component base color space linked here. Profiles inspect the base type and
 * table bounds for color-space validity.</p>
 *
 * <p><b>.NET porting notes:</b> Suggested type:
 * {@code VeraPdf.Validation.Gf.Model.Impl.Pd.Colors.GFPDIndexed}, implementing
 * {@code VeraPdf.Model.PDLayer.IPDIndexed}. Resolve the base through a centralized
 * color-space factory and expose it as {@code IReadOnlyList<IPDColorSpace>}.</p>
 *
 * @author Maksim Bezrukov
 */
public class GFPDIndexed extends GFPDColorSpace implements PDIndexed {

    /** Model type name for an Indexed color space. */
    public static final String INDEXED_TYPE = "PDIndexed";

    /** Link name for the color space used to interpret indexed lookup entries. */
    public static final String BASE = "base";

    /** Creates the model wrapper for the parser's Indexed space. */
    public GFPDIndexed(
            org.verapdf.pd.colors.PDIndexed simplePDObject) {
        super(simplePDObject, INDEXED_TYPE);
    }

    @Override
    /** Dispatches the {@code base} link; other links are handled by the resource base. */
    public List<? extends Object> getLinkedObjects(String link) {
        if (BASE.equals(link)) {
            return this.getBase();
        }
        return super.getLinkedObjects(link);
    }

	/** Resolves the underlying base color space, returning no link if unavailable. */
	private List<PDColorSpace> getBase() {
        org.verapdf.pd.colors.PDColorSpace baseColorSpace =
                ((org.verapdf.pd.colors.PDIndexed) this.simplePDObject).getBase();
        PDColorSpace colorSpace = ColorSpaceFactory.getColorSpace(baseColorSpace);
        if (colorSpace != null) {
            List<PDColorSpace> base = new ArrayList<>(MAX_NUMBER_OF_ELEMENTS);
            base.add(colorSpace);
            return Collections.unmodifiableList(base);
        }
        return Collections.emptyList();
    }
}
