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
import org.verapdf.cos.COSArray;
import org.verapdf.cos.COSObjType;
import org.verapdf.cos.COSObject;
import org.verapdf.gf.model.factory.operators.RenderingMode;
import org.verapdf.model.pdlayer.PDTrueTypeFont;
import org.verapdf.pd.font.FontProgram;
import org.verapdf.pd.font.opentype.OpenTypeFontProgram;
import org.verapdf.pd.font.truetype.AdobeGlyphList;
import org.verapdf.pd.font.truetype.TrueTypeFontProgram;

import java.io.IOException;
import java.util.logging.Level;
import java.util.logging.Logger;

/**
 * Adapts a simple TrueType font and checks its Unicode encoding metadata.
 *
 * <p>TrueType fonts use glyph indices and may embed a TrueType or OpenType program.
 * The Unicode-compliance check requires the Microsoft Unicode cmap subtable and verifies
 * encoding {@code /Differences} names against the Adobe Glyph List. A missing embedded
 * program does not fail this particular check; a present but unreadable program does.</p>
 *
 * <p><b>.NET porting notes:</b> Suggested type:
 * {@code VeraPdf.Validation.Gf.Model.Impl.Pd.Font.GFPDTrueTypeFont}, implementing
 * {@code VeraPdf.Model.PDLayer.IPDTrueTypeFont}. Use {@code PdfFontFactory} and
 * {@code TrueTypeFontInfo}; map {@code IOException} to {@code System.IO.IOException} and
 * logging to {@code ILogger<GFPDTrueTypeFont>}.</p>
 *
 * @author Sergey Shemyakov
 */
public class GFPDTrueTypeFont extends GFPDSimpleFont implements PDTrueTypeFont {

    /** Logger for font-program parsing diagnostics. */
    private static final Logger LOGGER = Logger.getLogger(GFPDTrueTypeFont.class.getCanonicalName());

    /** Model type name for a TrueType font dictionary. */
    public static final String TRUETYPE_FONT_TYPE = "PDTrueTypeFont";

    /** Wraps the TrueType dictionary and records whether its font program parses. */
    public GFPDTrueTypeFont(org.verapdf.pd.font.truetype.PDTrueTypeFont font,
                            RenderingMode renderingMode, String fontResourceName) {
        super(font, renderingMode, fontResourceName, TRUETYPE_FONT_TYPE);
        if (font != null) {
            this.fontProgramParsed = font.parseFontProgram();
        }
    }

    /** Checks the Unicode cmap and Adobe Glyph List names required for compliant Differences. */
    @Override
    public Boolean getdifferencesAreUnicodeCompliant() {
        if (pdFont.getFontProgram() == null) {
            return true;
        }
        if (!fontProgramParsed) {
            return Boolean.FALSE;
        }

        FontProgram font = this.pdFont.getFontProgram();
        if ((font instanceof OpenTypeFontProgram)) {
            font = ((OpenTypeFontProgram) font).getFont();
        }
        if (!((TrueTypeFontProgram) font).isCmapPresent(3, 1)) {
            return Boolean.FALSE;
        }
        COSObject encoding = this.pdFont.getEncoding();
        if (!encoding.empty() && encoding.getType() != COSObjType.COS_NAME) {
            COSObject differences = encoding.getKey(ASAtom.DIFFERENCES);
            if (!differences.empty() && differences.getType() == COSObjType.COS_ARRAY) {
                for (COSObject diff : (COSArray) differences.getDirectBase()) {
                    if (diff.getType() == COSObjType.COS_NAME &&
                            !AdobeGlyphList.contains(diff.getString())) {
                        return Boolean.FALSE;
                    }
                }
            }
        }

        return Boolean.TRUE;
    }

    /** TrueType fonts are not one of the fourteen standard Type1 base fonts. */
    @Override
    public Boolean getisStandard() {
        return Boolean.FALSE;
    }
}
