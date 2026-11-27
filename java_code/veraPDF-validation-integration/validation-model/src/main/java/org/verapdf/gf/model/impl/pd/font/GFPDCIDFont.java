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

import org.verapdf.as.io.ASInputStream;
import org.verapdf.cos.COSObjType;
import org.verapdf.cos.COSObject;
import org.verapdf.cos.COSStream;
import org.verapdf.gf.model.factory.operators.RenderingMode;
import org.verapdf.gf.model.impl.containers.StaticContainers;
import org.verapdf.model.operator.CIDGlyph;
import org.verapdf.model.operator.Glyph;
import org.verapdf.model.pdlayer.PDCIDFont;
import org.verapdf.pd.font.FontProgram;
import org.verapdf.pd.font.PDFont;
import org.verapdf.pdfa.flavours.PDFAFlavour;
import org.verapdf.pdfa.flavours.PDFFlavours;
import org.verapdf.tools.StaticResources;

import java.io.IOException;
import java.util.*;
import java.util.logging.Level;
import java.util.logging.Logger;

/**
 * Adapts a descendant CIDFont and validates its glyph-set and CID-to-GID mapping.
 *
 * <p>CIDFonts map character identifiers to glyphs and are commonly descendants of a
 * composite Type0 font. The adapter registers the embedded font program, checks the
 * optional CIDSet against glyphs in that program, and reports whether {@code /CIDToGIDMap}
 * is {@code /Identity} or a custom stream. The bit set is read most-significant-bit first;
 * ISO 19005-1 uses a separate rule path based on glyphs cached during rendering.</p>
 *
 * <p><b>.NET porting notes:</b></p>
 * <ul>
 *   <li>Suggested type: {@code VeraPdf.Validation.Gf.Model.Impl.Pd.Font.GFPDCIDFont},
 *       implementing {@code VeraPdf.Model.PDLayer.IPDCIDFont}.</li>
 *   <li>Use {@code VeraPdf.Parser.Fonts.PdfFont}, {@code PdfCMap}, and
 *       {@code TrueTypeFontInfo}; read decoded data through {@code Stream} and dispose it.</li>
 *   <li>Java bytes are signed while C# {@code byte} is unsigned; preserve the explicit
 *       0..255 conversion and the big-endian bit numbering when porting {@code BitSet} logic.</li>
 *   <li>Map nullable DSL integers to {@code long?}, and Java logging to
 *       {@code ILogger<GFPDCIDFont>}.</li>
 * </ul>
 *
 * @author Sergey Shemyakov
 */
public class GFPDCIDFont extends GFPDFont implements PDCIDFont {

    /** Logger for recoverable embedded-font parsing failures. */
    private static final Logger LOGGER = Logger.getLogger(GFPDCIDFont.class.getCanonicalName());

    /** Model type name for a CIDFont descendant. */
    public static final String CID_FONT_TYPE = "PDCIDFont";

    /** CID-to-GID mapping name for a direct identity map. */
    public static final String IDENTITY = "Identity";
    /** Reported value when CID-to-GID mapping is stored as a stream. */
    public static final String CUSTOM = "Custom";
    /** Maximum byte count read from a CIDSet stream. */
    public static final int maxSize = 16384;
    /** Chunk size used to read the CIDSet stream. */
    public static final int bufferSize = 2048;
    /** Stable external identifier used to find cached glyphs for this font. */
    private final String externalFontID;

    /** Wraps the CIDFont, registers its font-program resource, and parses the program. */
    public GFPDCIDFont(PDFont font, RenderingMode renderingMode, String externalFontID, String fontResourceName) {
        super(font, renderingMode, fontResourceName, CID_FONT_TYPE);
        this.externalFontID = externalFontID;
        if (font != null) {
            FontProgram program = font.getFontProgram();
            if (program != null) {
                StaticResources.getDocument().getDocument().getResourceHandler().addResource(
                        program.getFontProgramResource());
            }
            this.fontProgramParsed = font.parseFontProgram();
        }
    }

    /** Returns {@code Identity} for the name value or {@code Custom} for a stream. */
    @Override
    public String getCIDToGIDMap() {
        COSObject cidToGidObject =
                ((org.verapdf.pd.font.PDCIDFont) this.pdFont).getCIDToGIDMap();
        if (cidToGidObject.getType() == COSObjType.COS_STREAM) {
            return CUSTOM;
        }
        if (cidToGidObject.getType() == COSObjType.COS_NAME &&
                IDENTITY.equals(cidToGidObject.getString())) {
            return IDENTITY;
        }
        return null;
    }

    @Override
    /** Reports whether the descendant font dictionary contains a CIDSet stream. */
    public Boolean getcontainsCIDSet() {
        return ((org.verapdf.pd.font.PDCIDFont) this.pdFont).getCIDSet() != null;
    }

    /** Checks CIDSet membership against font-program glyphs using the active PDF/A rule path. */
    @Override
    public Boolean getcidSetListsAllGlyphs() {
        if (!fontProgramParsed) {
            return Boolean.FALSE;
        }

        try {
            COSStream cidSet = getCIDSetStream();
            if (cidSet != null) {
                byte[] cidSetBytes;
                try (ASInputStream stream = cidSet.getData(COSStream.FilterFlags.DECODE)) {
                    cidSetBytes = getCIDsFromCIDSet(stream);
                }

                //reverse bit order in bit set (convert to big endian)
                BitSet bitSet = toBitSetBigEndian(cidSetBytes);

                FontProgram cidFont = this.pdFont.getFontProgram();

                if (!PDFFlavours.isFlavourPart(StaticContainers.getFlavour(), PDFAFlavour.Specification.ISO_19005_1)) {
                    //on these levels we need to ensure that all glyphs present in font program are described in cid set
                    List<Integer> fontCIDs = cidFont != null ? cidFont.getCIDList() : Collections.emptyList();
                    for (int cid : fontCIDs) {
                        if (cid != 0 && !bitSet.get(cid)) {
                            return Boolean.FALSE;
                        }
                    }
                    // we skip i = 0 which corresponds to .notdef glyph
                    for (int i = 1; i < bitSet.length(); i++) {
                        if (bitSet.get(i) && !cidFont.containsCID(i)) {
                            return Boolean.FALSE;
                        }
                    }
                } else {
                    Map<String, Glyph> map = StaticContainers.getCachedGlyphs().get(externalFontID);
                    if (map != null) {
                        for (Glyph glyph : map.values()) {
                            if (glyph instanceof CIDGlyph) {
                                int cid = ((CIDGlyph)glyph).getCID().intValue();
                                if (cid != 0 && cidFont.containsCID(cid) && !bitSet.get(cid)) {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
        } catch (IOException e) {
            LOGGER.log(Level.FINE, "Error while parsing embedded font program. " + e.getMessage());
            return Boolean.FALSE;
        }
        return Boolean.TRUE;
    }

	/** Returns the optional CIDSet stream from the descendant font dictionary. */
	private COSStream getCIDSetStream() {
        return ((org.verapdf.pd.font.PDCIDFont) this.pdFont).getCIDSet();
    }

	/** Reads at most the configured CIDSet limit, stopping at end-of-stream or cap. */
	private static byte[] getCIDsFromCIDSet(ASInputStream cidSet) throws IOException {
        byte[] cidSetBytes = new byte[maxSize];
        byte[] temp = new byte[bufferSize];
        int size = 0;
        int read = cidSet.read(temp, bufferSize);
        while (read != -1) {
            if (size + read >= maxSize) {
                System.arraycopy(temp, 0, cidSetBytes, size, maxSize - size);
                return cidSetBytes;
            }
            System.arraycopy(temp, 0, cidSetBytes, size, read);
            size += read;
            if (read < bufferSize) {
                return Arrays.copyOf(cidSetBytes, size);
            }
            read = cidSet.read(temp, bufferSize);
        }
        return Arrays.copyOf(cidSetBytes, size);
    }

	/** Expands bytes into a bit set in PDF CIDSet bit order (most significant bit first). */
	private static BitSet toBitSetBigEndian(byte[] source) {
        BitSet bitSet = new BitSet(source.length * 8);
        int i = 0;
        for (byte value : source) {
            int b = value >= 0 ? value : 256 + value;
            for (int k = 0; k < 8; k++) {
                bitSet.set(i++, (b & 0x80) != 0);
                b = b << 1;
            }
        }
        return bitSet;
    }
}
