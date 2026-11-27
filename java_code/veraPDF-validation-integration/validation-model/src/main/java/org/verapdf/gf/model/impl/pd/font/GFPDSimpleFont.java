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
package org.verapdf.gf.model.impl.pd.font;

import org.verapdf.as.ASAtom;
import org.verapdf.cos.COSObjType;
import org.verapdf.cos.COSObject;
import org.verapdf.gf.model.factory.operators.RenderingMode;
import org.verapdf.model.pdlayer.PDSimpleFont;

/**
 * Shared model behavior for simple fonts: Type1, TrueType, and Type3.
 *
 * <p>Simple fonts map one-byte character codes through {@code /FirstChar}, widths, and
 * an encoding that may contain {@code /Differences}. This base reports the range and
 * width-array size, resolves the base encoding, and leaves standard-font classification
 * to the subtype. Type0 composite fonts use a different mapping through CMaps and CIDFonts.</p>
 *
 * <p><b>.NET porting notes:</b> Suggested base:
 * {@code VeraPdf.Validation.Gf.Model.Impl.Pd.Font.GFPDSimpleFont}, implementing
 * {@code VeraPdf.Model.PDLayer.IPDSimpleFont}. Read encoding arrays via parser COS types;
 * preserve null for absent or malformed entries and map DSL integers to {@code long?}.</p>
 *
 * @author Sergey Shemyakov
 */
public abstract class GFPDSimpleFont extends GFPDFont implements PDSimpleFont {

    /** Initializes the shared simple-font adapter with source and rendering context. */
    protected GFPDSimpleFont(org.verapdf.pd.font.PDSimpleFont font,
                             RenderingMode renderingMode, String fontResourceName, final String type) {
        super(font, renderingMode, fontResourceName, type);
    }

    /** Reports whether the subtype is one of the standard PDF base fonts. */
    @Override
    public abstract Boolean getisStandard();

    /** Returns {@code /FirstChar}, or null when absent or malformed. */
    @Override
    public Long getFirstChar() {
        return this.pdFont.getFirstChar();
    }

    /** Returns {@code /LastChar}, or null when absent or malformed. */
    @Override
    public Long getLastChar() {
        return this.pdFont.getLastChar();
    }

    /** Returns the {@code /Widths} array length, or null when the entry is invalid. */
    @Override
    public Long getWidths_size() {
        COSObject widths = this.pdFont.getWidths();
        if (widths.empty() || widths.getType() != COSObjType.COS_ARRAY) {
            return null;
        }
        return (widths.getDirectBase()).size().longValue();
    }

    /** Returns the encoding name or dictionary base encoding; absent values remain null. */
    @Override
    public String getEncoding() {
        COSObject encoding = this.pdFont.getEncoding();
        if (encoding.empty()) {
            return null;
        }
        if (encoding.getType() == COSObjType.COS_NAME) {
            return encoding.getString();
        }
        return encoding.getKey(ASAtom.BASE_ENCODING).getString();
    }

    @Override
    /** Reports whether an encoding dictionary explicitly contains {@code /Differences}. */
    public Boolean getcontainsDifferences() {
        COSObject encoding = this.pdFont.getEncoding();
        return encoding.getType() == COSObjType.COS_DICT && encoding.knownKey(ASAtom.DIFFERENCES);
    }
}
