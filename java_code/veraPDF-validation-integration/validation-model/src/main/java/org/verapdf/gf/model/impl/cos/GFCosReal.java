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

import org.verapdf.cos.COSNumber;
import org.verapdf.model.coslayer.CosReal;

/**
 * Specializes the generic COS number adapter for a PDF real-valued object.
 *
 * <p>Real values occur in coordinates, color components, and other numeric operands; conversion and exposed
 * numeric properties are shared with {@link GFCosNumber}.</p>
 *
 * <p><b>.NET porting notes:</b> Implement {@code VeraPdf.Model.CosLayer.ICosReal} over
 * {@code VeraPdf.Parser.Cos.CosReal}; preserve the {@code double?} value and the base class's integer projection
 * semantics when porting to C#.</p>
 *
 * @author Timur Kamalov
 */
public class GFCosReal extends GFCosNumber implements CosReal {

    /** Type name for GFCosReal */
    public static final String COS_REAL_TYPE = "CosReal";

    /**
     * Default constructor
     * @param value greenfield COSNumber
     */
    public GFCosReal(COSNumber value) {
        super(value, COS_REAL_TYPE);
    }

}
