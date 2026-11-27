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
import org.verapdf.cos.COSName;
import org.verapdf.gf.model.factory.operators.RenderingMode;
import org.verapdf.gf.model.impl.cos.GFCosUnicodeName;
import org.verapdf.gf.model.impl.external.GFFontProgram;
import org.verapdf.gf.model.impl.external.GFTrueTypeFontProgram;
import org.verapdf.gf.model.impl.pd.GFPDResource;
import org.verapdf.model.baselayer.Object;
import org.verapdf.model.coslayer.CosUnicodeName;
import org.verapdf.model.external.FontProgram;
import org.verapdf.model.pdlayer.PDFont;
import org.verapdf.pd.font.Encoding;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

/**
 * Common PD-model adapter for PDF font dictionaries and font-program metadata.
 *
 * <p>Font dictionaries describe simple fonts (Type1, TrueType, Type3) or composite Type0
 * fonts with descendant CIDFonts. The adapter exposes subtype, naming, symbolic/italic
 * flags, rendering mode, an optional embedded program, and the {@code BaseFont} name.
 * Font program parsing is gated by {@code fontProgramParsed}; specialized subclasses
 * decide how parser programs are checked and presented.</p>
 *
 * <p><b>.NET porting notes:</b></p>
 * <ul>
 *   <li>Suggested base: {@code VeraPdf.Validation.Gf.Model.Impl.Pd.Font.GFPDFont},
 *       implementing {@code VeraPdf.Model.PDLayer.IPDFont}.</li>
 *   <li>Use {@code VeraPdf.Parser.Fonts.PdfFont}, {@code PdfFontFactory},
 *       {@code PdfCMap}, and {@code TrueTypeFontInfo}; expose font-program links through
 *       {@code VeraPdf.Model.External.IFontProgram}.</li>
 *   <li>Preserve lazy/conditional font-program exposure and nullable properties; Java
 *       {@code Long} maps to {@code long?}. Replace collection wrappers with read-only lists.</li>
 * </ul>
 *
 * @author Sergey Shemyakov
 */
public class GFPDFont extends GFPDResource implements PDFont {

    /** Link name for the parsed embedded font program. */
    public static final String FONT_FILE = "fontFile";
    /** Link name for the font dictionary's base name object. */
    public static final String BASE_FONT = "BaseFont";
    /** PDF subtype name used to detect composite Type0 fonts. */
    private static final String TYPE0_STRING = "Type0";

    /** Whether parsing the embedded program succeeded; controls link exposure. */
    protected boolean fontProgramParsed;
    /** Rendering mode inherited from the graphics state at font selection. */
    protected final RenderingMode renderingMode;
    /** Resource name under which this font was selected. */
    private final String resourceName;

    /** Initializes shared font properties and the profile-visible type name. */
    protected GFPDFont(org.verapdf.pd.font.PDFont font,
                       RenderingMode renderingMode, String resourceName, final String type) {
        super(font, type);
        this.renderingMode = renderingMode;
        this.resourceName = resourceName;
    }

    /** Returns the font dictionary's {@code /Type} value. */
    @Override
    public String getType() {
        return this.pdFont.getType();
    }

    /** Returns the font dictionary's {@code /Subtype} name, or null if absent. */
    @Override
    public String getSubtype() {
        ASAtom result = this.pdFont.getSubtype();
        return result != null ? result.getValue() : null;
    }

    /** Returns the parser-resolved font name from the dictionary/descriptor. */
    @Override
    public String getfontName() {
        return this.pdFont.getName();
    }

    /** Returns whether the descriptor flags classify this font as symbolic. */
    @Override
    public Boolean getisSymbolic() {
        return this.pdFont.isSymbolic();
    }

    /** Returns the active text rendering mode as a model integer. */
    @Override
    public Long getrenderingMode() {
        return (long) this.renderingMode.getValue();
    }

    @Override
    /** Returns the embedded font-file subtype when a source font is available. */
    public String getfontFileSubtype() {
        if (this.pdFont != null) {
            ASAtom subtype = this.pdFont.getProgramSubtype();
            if (subtype != null) {
                return subtype.getValue();
            }
        }
        return null;
    }

    @Override
    /** Returns the descriptor italic flag, or null when no descriptor exists. */
    public Boolean getisItalic() {
        org.verapdf.pd.font.PDFontDescriptor fontDescriptor = this.pdFont.getFontDescriptor();
        if (fontDescriptor != null) {
            return fontDescriptor.isItalic();
        }
        return null;
    }

    @Override
    /** Dispatches the font program and BaseFont links. */
    public List<? extends Object> getLinkedObjects(String link) {
        switch (link) {
            case FONT_FILE:
                return this.getFontProgram();
            case BASE_FONT:
                return this.getBaseFont();
            default:
                return super.getLinkedObjects(link);
        }
    }

    /** Reports whether the source font program parsed successfully. */
    public boolean isFontProgramParsed() {
        return this.fontProgramParsed;
    }

    /** Creates the correct external font-program adapter after successful parsing. */
    private List<FontProgram> getFontProgram() {
        org.verapdf.pd.font.FontProgram fontProgram = this.pdFont.getFontProgram();
        if (fontProgram != null && this.fontProgramParsed) {
            ASAtom subType = this.pdFont.getSubtype();
            if (ASAtom.TRUE_TYPE == subType) {
                GFTrueTypeFontProgram font = new GFTrueTypeFontProgram(
                        fontProgram);
                return getFontProgramList(font);
            }
			if (TYPE0_STRING.equals(this.getSubtype())) {
			    GFFontProgram font = new GFFontProgram(fontProgram,
			            (GFPDFont) ((GFPDType0Font) this).getDescendantFont());
			    return getFontProgramList(font);
			}
			GFFontProgram font = new GFFontProgram(fontProgram,
			        this);
			return getFontProgramList(font);
        }
		return Collections.emptyList();
    }

	/** Returns a single program as an immutable model link list. */
	private static List<FontProgram> getFontProgramList(FontProgram fontProgram) {
        List<FontProgram> list = new ArrayList<>(MAX_NUMBER_OF_ELEMENTS);
        list.add(fontProgram);
        return Collections.unmodifiableList(list);
    }

    @Override
    /** Reports whether a program exists and was parsed successfully. */
    public Boolean getcontainsFontFile() {
        return this.pdFont.getFontProgram() != null && this.fontProgramParsed;
    }

    /** Wraps the resolved font name as a COS Unicode-name model object. */
    private List<CosUnicodeName> getBaseFont() {
        String name = this.pdFont.getName();
        if (name != null) {
            List<CosUnicodeName> list = new ArrayList<>(MAX_NUMBER_OF_ELEMENTS);
            list.add(new GFCosUnicodeName((COSName)
                    COSName.construct(ASAtom.getASAtom(name)).get()));
            return Collections.unmodifiableList(list);
        }
        return Collections.emptyList();
    }

    /** Exposes parser encoding metadata to font-specific adapters. */
    public Encoding getEncodingMapping() {
        return this.pdFont.getEncodingMapping();
    }
    
    @Override
    /** Returns the PDF resource name associated with this font in its content stream. */
    public String getExtraContext() {
        return resourceName;
    } 
}
