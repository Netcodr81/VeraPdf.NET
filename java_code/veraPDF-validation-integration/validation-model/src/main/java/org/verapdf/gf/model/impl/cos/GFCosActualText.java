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
import org.verapdf.model.coslayer.CosActualText;

/**
 * Exposes a PDF string used as replacement text for marked-content sequences.
 *
 * <p>In tagged PDF, the {@code ActualText} entry supplies text to assistive technology and text extraction when
 * the visible glyphs do not adequately represent the intended content. The COS visitor creates this specialized
 * wrapper so model consumers can distinguish it from an ordinary string while retaining the same byte data.</p>
 *
 * <p><b>.NET porting notes:</b> Add {@code VeraPdf.Validation.Gf.Model.Impl.Cos.GFCosActualText}, implementing
 * {@code VeraPdf.Model.CosLayer.ICosActualText}. Reuse {@code VeraPdf.Parser.Cos.CosString}; keep the model type
 * discriminator distinct from ordinary strings and preserve the base string's decoding behavior.</p>
 *
 * @author Maxim Plushchov
 */
public class GFCosActualText extends GFCosString implements CosActualText {

    public static final String COS_ACTUAL_TEXT_TYPE = "CosActualText";

    public GFCosActualText(COSString cosString) {
        super(cosString, COS_ACTUAL_TEXT_TYPE);
    }
}
