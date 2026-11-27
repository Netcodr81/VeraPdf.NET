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
import org.verapdf.model.coslayer.CosLang;

/**
 * Represents a PDF language identifier string, typically an RFC 3066 language tag.
 *
 * <p>Language values let PDF content or structure identify the natural language needed for pronunciation and
 * text processing. This specialization inherits PDF text-string decoding while exposing the distinct
 * {@code CosLang} model type.</p>
 *
 * <p><b>.NET porting notes:</b> Implement {@code VeraPdf.Model.CosLayer.ICosLang} over
 * {@code VeraPdf.Parser.Cos.CosString}; preserve the raw/decoded string distinction of the base adapter and
 * keep the optional subtype constructor behavior.</p>
 *
 * @author Sergey Shemyakov
 */
public class GFCosLang extends GFCosTextString implements CosLang {

    /** Type name for GFCosLang */
    public static final String COS_LANG_TYPE = "CosLang";

    /**
     * Default constructor
     * @param cosString is Lang object COSString.
     */
    public GFCosLang(COSString cosString) {
        this(cosString, COS_LANG_TYPE);
    }

    /**
     * Constructor for child classes
     * @param cosString is Lang COSString.
     * @param type child class type.
     */
    public GFCosLang(COSString cosString, final String type) {
        super(cosString, type);
    }

}
