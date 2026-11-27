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

import org.verapdf.cos.COSString;
import org.verapdf.model.coslayer.CosAlt;

/**
 * Wraps the alternate text associated with a PDF structure element or marked-content sequence.
 *
 * <p>The {@code Alt} value is descriptive text used when a non-text object needs an accessible alternative;
 * this subclass preserves the COS string representation but gives validation profiles a specific model type.</p>
 *
 * <p><b>.NET porting notes:</b> Implement {@code VeraPdf.Validation.Gf.Model.Impl.Cos.GFCosAlt} as
 * {@code VeraPdf.Model.CosLayer.ICosAlt}, backed by {@code VeraPdf.Parser.Cos.CosString}. Keep its semantic
 * distinction from {@code ActualText}; both happen to use PDF strings but serve different accessibility roles.</p>
 *
 * @author Maxim Plushchov
 */
public class GFCosAlt extends GFCosString implements CosAlt {

    public static final String COS_ALT_TYPE = "CosAlt";

    public GFCosAlt(COSString cosString) {
        super(cosString, COS_ALT_TYPE);
    }
}
