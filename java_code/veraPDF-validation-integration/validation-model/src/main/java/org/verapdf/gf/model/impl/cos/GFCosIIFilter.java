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
package org.verapdf.gf.model.impl.cos;

import org.verapdf.cos.COSName;
import org.verapdf.model.coslayer.CosIIFilter;

/**
 * Wraps the inline-image filter name used in a PDF content stream.
 *
 * <p>Inline images use abbreviated filter names in their BI/ID/EI syntax, but the model exposes a dedicated
 * {@code CosIIFilter} type so image validation rules can inspect the decoded name consistently.</p>
 *
 * <p><b>.NET porting notes:</b> Implement {@code VeraPdf.Model.CosLayer.ICosIIFilter} as a specialized adapter
 * over {@code VeraPdf.Parser.Cos.CosName}; convert string input through the parser's name factory and preserve
 * name decoding/normalization semantics.</p>
 *
 * @author Timur Kamalov
 */
public class GFCosIIFilter extends GFCosName implements CosIIFilter {

    public static final String COS_II_FILTER_TYPE = "CosIIFilter";

    public GFCosIIFilter(final COSName filter) {
        super(filter, COS_II_FILTER_TYPE);
    }

    public GFCosIIFilter(final String filter) {
        this((COSName) COSName.fromValue(filter));
    }

}
