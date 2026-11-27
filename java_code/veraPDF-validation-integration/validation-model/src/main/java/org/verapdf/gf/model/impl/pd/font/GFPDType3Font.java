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
import org.verapdf.cos.COSDictionary;
import org.verapdf.cos.COSKey;
import org.verapdf.cos.COSObjType;
import org.verapdf.cos.COSObject;
import org.verapdf.gf.model.factory.operators.GraphicState;
import org.verapdf.gf.model.factory.operators.RenderingMode;
import org.verapdf.gf.model.impl.containers.StaticContainers;
import org.verapdf.gf.model.impl.pd.GFPDContentStream;
import org.verapdf.pd.PDResourcesHandler;
import org.verapdf.model.baselayer.Object;
import org.verapdf.model.pdlayer.PDContentStream;
import org.verapdf.model.pdlayer.PDType3Font;
import org.verapdf.pd.PDResources;
import org.verapdf.pd.font.type3.PDType3CharProc;
import org.verapdf.pd.structure.StructureElementAccessObject;
import org.verapdf.pdfa.flavours.PDFAFlavour;
import org.verapdf.pdfa.flavours.PDFFlavours;

import java.util.*;
import java.util.logging.Level;
import java.util.logging.Logger;

/**
 * Adapts a Type3 font and lazily exposes each glyph's CharProc content stream.
 *
 * <p>Type3 fonts define glyphs as PDF content streams rather than relying solely on an
 * embedded font program. The CharProcs dictionary maps glyph names to streams; each is
 * wrapped as content using its own resources when present, otherwise the font's inherited
 * resources, and with inherited graphics state and structure access. Parsing is lazy so
 * unused glyph streams are not traversed until a rule requests {@code charStrings}.</p>
 *
 * <p><b>.NET porting notes:</b></p>
 * <ul>
 *   <li>Suggested type: {@code VeraPdf.Validation.Gf.Model.Impl.Pd.Font.GFPDType3Font},
 *       implementing {@code VeraPdf.Model.PDLayer.IPDType3Font}.</li>
 *   <li>Use {@code PdfContentStream}, {@code PdfResources}, and COS dictionary/stream
 *       types from {@code VeraPdf.Parser.Pd} / {@code VeraPdf.Parser.Cos}; represent the
 *       CharProcs cache as a dictionary plus read-only link list.</li>
 *   <li>Preserve lazy initialization and resource fallback. PDF/A-4 rejects resource
 *       dictionaries on individual CharProc streams, as noted in the resource helper.</li>
 * </ul>
 *
 * @author Sergey Shemyakov
 */
public class GFPDType3Font extends GFPDSimpleFont implements PDType3Font {

    /** Logger for malformed CharProcs entries. */
    private static final Logger LOGGER = Logger.getLogger(GFPDType3Font.class.getCanonicalName());

    /** Model type name for Type3 fonts. */
    public static final String TYPE3_FONT_TYPE = "PDType3Font";

    /** Link name for the glyph-name-to-content-stream collection. */
    public static final String CHAR_STRINGS = "charStrings";
    /** Page/resource resolver inherited by glyph programs when they declare none. */
    private final PDResourcesHandler resources;
    /** Lazily populated map of glyph names to their CharProc content model. */
    private Map<String, PDContentStream> charStrings = null;
    /** Graphics state inherited by glyph content streams. */
    private final GraphicState inheritedGraphicState;

    /** Captures the resource and graphics-state context needed to parse glyph streams. */
    public GFPDType3Font(org.verapdf.pd.font.type3.PDType3Font font,
                         RenderingMode renderingMode, PDResourcesHandler resources,
                         GraphicState inheritedGraphicState, String fontResourceName) {
        super(font, renderingMode, fontResourceName, TYPE3_FONT_TYPE);
        this.resources = resources;
        this.inheritedGraphicState = inheritedGraphicState;
    }

    /** Type3 fonts are not standard base fonts. */
    @Override
    public Boolean getisStandard() {
        return Boolean.FALSE;
    }

    @Override
    /** Resolves the lazily parsed {@code charStrings} link. */
    public List<? extends Object> getLinkedObjects(String link) {
        if (CHAR_STRINGS.equals(link)) {
            return this.getCharStrings();
        }
        return super.getLinkedObjects(link);
    }

    /** Returns all glyph CharProc content streams, parsing them on first access. */
    private List<PDContentStream> getCharStrings() {
        if (this.charStrings == null) {
            parseCharStrings();
        }
        return new ArrayList<>(this.charStrings.values());
    }

    /** Returns the glyph map as an immutable view, parsing it if needed. */
    public Map<String, PDContentStream> getCharProcStreams() {
        if (this.charStrings == null) {
            parseCharStrings();
        }
        return Collections.unmodifiableMap(this.charStrings);
    }

	/** Parses each valid CharProc dictionary entry and caches its content adapter. */
	private void parseCharStrings() {
        COSDictionary charProcDict = ((org.verapdf.pd.font.type3.PDType3Font)
                this.pdFont).getCharProcDict();
        if (charProcDict != null) {
            COSKey objectKey = this.pdFont.getKey(ASAtom.CHAR_PROCS).getObjectKey();
            Set<ASAtom> keySet = charProcDict.getKeySet();
            Map<String, PDContentStream> map = new HashMap<>(keySet.size());
            for (ASAtom glyphName : keySet) {
                COSObject charProcStream = charProcDict.getKey(glyphName);
                if (!charProcStream.empty() && charProcDict.getType() == COSObjType.COS_DICT) {
                    PDType3CharProc charProc = new PDType3CharProc(charProcStream);
                    PDResourcesHandler glyphResources = getResourcesFromCharProcs(charProcStream);
                    GFPDContentStream contentStream =
                            new GFPDContentStream(charProc, glyphResources == null ?
                                    this.resources : glyphResources, inheritedGraphicState,
                                    new StructureElementAccessObject(this.simpleCOSObject), objectKey);
                    map.put(glyphName.getValue(), contentStream);
                } else {
                    LOGGER.log(Level.SEVERE, "Invalid entry in the char proc dictionary, dictionary is expected.");
                }
            }
            this.charStrings = Collections.unmodifiableMap(map);
        } else {
            this.charStrings = Collections.emptyMap();
        }
    }

    /** Resolves per-CharProc resources while retaining fallback to the font's resource context. */
    private PDResourcesHandler getResourcesFromCharProcs(COSObject charProcs) {
        if (!charProcs.knownKey(ASAtom.RESOURCES)) {
            return null;
        }
        PDResources res = new PDResources(charProcs.getKey(ASAtom.RESOURCES));
        return PDResourcesHandler.getInstance(this.resources.getPageResources(), res);
    }
}
