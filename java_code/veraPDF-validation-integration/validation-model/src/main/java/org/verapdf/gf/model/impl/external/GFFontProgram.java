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
package org.verapdf.gf.model.impl.external;

import org.verapdf.gf.model.impl.pd.font.GFPDFont;
import org.verapdf.model.external.FontProgram;

/**
 * Common external-model wrapper for a font program embedded in a PDF font.
 *
 * <p>The wrapper associates the parsed font bytes/program with the owning PDF font and derives a stable model ID
 * from that font. Concrete subclasses expose format-specific checks, for example TrueType cmap-table properties.</p>
 *
 * <p><b>.NET porting notes:</b> Implement {@code VeraPdf.Model.External.IFontProgram}; use
 * {@code VeraPdf.Parser.Fonts.PdfFont} and {@code TrueTypeFontInfo} for parser-side font data. Preserve IDs as
 * strings and ensure parsing exceptions leave model properties in a documented nullable or invalid state.</p>
 *
 * @author Sergey Shemyakov
 */
public class GFFontProgram extends GFExternal implements FontProgram {

    public static final String FONT_PROGRAM_TYPE = "FontProgram";

    protected GFFontProgram(String type) {
        super(type);
    }

    private String id;
    private org.verapdf.pd.font.FontProgram fontProgram;

    public GFFontProgram(org.verapdf.pd.font.FontProgram fontProgram,
                         GFPDFont font) {
        super(FONT_PROGRAM_TYPE);
        this.fontProgram = fontProgram;
        this.id = font.getID() + " font program";
    }

    @Override
    public String getID() {
        return this.id;
    }
}
