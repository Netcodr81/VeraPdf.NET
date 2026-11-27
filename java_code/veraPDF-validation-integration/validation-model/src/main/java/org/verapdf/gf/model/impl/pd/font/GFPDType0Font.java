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
import org.verapdf.gf.model.factory.operators.RenderingMode;
import org.verapdf.gf.model.impl.pd.GFPDObject;
import org.verapdf.gf.model.tools.GFIDGenerator;
import org.verapdf.model.baselayer.Object;
import org.verapdf.model.pdlayer.PDCIDFont;
import org.verapdf.model.pdlayer.PDCMap;
import org.verapdf.model.pdlayer.PDType0Font;
import org.verapdf.pd.font.PDCIDSystemInfo;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import java.util.logging.Level;
import java.util.logging.Logger;

/**
 * Adapts a Type0 composite font and its descendant CIDFont and encoding CMap.
 *
 * <p>Type0 fonts decode multibyte character codes through an Encoding CMap, then map
 * character identifiers to glyphs through a descendant CIDFont. Construction creates
 * the descendant model adapter, shares its font program with the Type0 parser object,
 * and propagates parse status. Registry, ordering, and supplement properties compare
 * the CMap with the descendant's CIDSystemInfo.</p>
 *
 * <p><b>.NET porting notes:</b></p>
 * <ul>
 *   <li>Suggested type: {@code VeraPdf.Validation.Gf.Model.Impl.Pd.Font.GFPDType0Font},
 *       implementing {@code VeraPdf.Model.PDLayer.IPDType0Font}.</li>
 *   <li>Use {@code PdfFont}, {@code PdfCMap}, {@code CMapParser}, and
 *       {@code PdfFontFactory}; expose {@code DescendantFonts} and {@code Encoding} via
 *       {@code IReadOnlyList<IModelObject>} links.</li>
 *   <li>Registry/order getters assume both CIDSystemInfo and CMap are present; preserve
 *       source validity assumptions or add explicit validation before dereferencing.</li>
 * </ul>
 *
 * @author Sergey Shemyakov
 */
public class GFPDType0Font extends GFPDFont implements PDType0Font {

	/** Logger for missing CIDSystemInfo and Encoding metadata. */
	private static final Logger LOGGER = Logger.getLogger(GFPDType0Font.class.getCanonicalName());

	/** Model type name for a Type0 composite font. */
	public static final String TYPE_0_FONT_TYPE = "PDType0Font";

	/** Link name for the descendant CIDFont dictionary. */
	public static final String DESCENDANT_FONTS = "DescendantFonts";
	/** Link name for the Type0 font's encoding CMap. */
	public static final String ENCODING = "Encoding";

	/** Adapted descendant font; null if the source dictionary lacks one. */
	private final PDCIDFont descendantFont;
	/** Parser-side descendant font retained for access to CIDSystemInfo. */
	private org.verapdf.pd.font.PDCIDFont cidFont;

	/** Builds the descendant CIDFont and synchronizes composite font parse status. */
	public GFPDType0Font(org.verapdf.pd.font.PDType0Font font, RenderingMode renderingMode, String fontResourceName) {
		super(font, renderingMode, fontResourceName, TYPE_0_FONT_TYPE);
		this.descendantFont = this.calculateDescendantFont();
		((org.verapdf.pd.font.PDType0Font) this.pdFont).setFontProgramFromDescendant(this.cidFont);
		this.fontProgramParsed = this.descendantFont != null
				&& ((GFPDCIDFont) this.descendantFont).isFontProgramParsed();
		this.pdFont.setSuccessfullyParsed(this.fontProgramParsed);
	}

	@Override
	/** Dispatches the descendant and encoding links. */
	public List<? extends Object> getLinkedObjects(String link) {
		switch (link) {
		case DESCENDANT_FONTS:
			return this.getDescendantFonts();
		case ENCODING:
			return this.getEncoding();
		default:
			return super.getLinkedObjects(link);
		}
	}

    /** Returns the descendant CIDFont link, empty when the dictionary is malformed. */
    private List<PDCIDFont> getDescendantFonts() {
		if (this.descendantFont != null) {
			List<PDCIDFont> list = new ArrayList<>(GFPDObject.MAX_NUMBER_OF_ELEMENTS);
			list.add(descendantFont);
			return Collections.unmodifiableList(list);
		}
		return Collections.emptyList();
	}

	/** Exposes the already-created descendant adapter to the generic font wrapper. */
	public PDCIDFont getDescendantFont() {
		return this.descendantFont;
	}

	/** Creates a CIDFont from the descendant dictionary and parent CMap/program. */
	private PDCIDFont calculateDescendantFont() {
		COSDictionary cidFontDict = ((org.verapdf.pd.font.PDType0Font) this.pdFont).getDescendantFont();
		if (cidFontDict != null) {
			org.verapdf.pd.font.PDCIDFont cidFont = new org.verapdf.pd.font.PDCIDFont(cidFontDict,
					((org.verapdf.pd.font.PDType0Font) this.pdFont).getCMap().getCMapFile(),
					this.pdFont.getFontProgram(), this.pdFont.isSuccessfullyParsed());
			this.cidFont = cidFont;
			return new GFPDCIDFont(cidFont, renderingMode, GFIDGenerator.generateID(this.pdFont), null);
		}
		return null;
	}

    /** Wraps the font's CMap as the Encoding link when one exists. */
    private List<PDCMap> getEncoding() {
		if (((org.verapdf.pd.font.PDType0Font) this.pdFont).getCMap() != null) {
			PDCMap res = new GFPDCmap(((org.verapdf.pd.font.PDType0Font) this.pdFont).getCMap());
			List<PDCMap> list = new ArrayList<>(GFPDObject.MAX_NUMBER_OF_ELEMENTS);
			list.add(res);
			return Collections.unmodifiableList(list);
		}
		return Collections.emptyList();
	}

    @Override
    /** Returns the descendant CIDSystemInfo ordering, or null when it is absent. */
    public String getCIDFontOrdering() {
		PDCIDSystemInfo info = ((org.verapdf.pd.font.PDType0Font) this.pdFont).getCIDSystemInfo();
		if (info == null) {
			LOGGER.log(Level.FINE, "CID font dictionary doesn't contain CIDSystemInfo");
			return null;
		}
		return info.getStringKey(ASAtom.ORDERING);
	}

    @Override
    /** Returns the CMap ordering, or null when the Encoding map is absent. */
    public String getCMapOrdering() {
		org.verapdf.pd.font.cmap.PDCMap cmap = ((org.verapdf.pd.font.PDType0Font) this.pdFont).getCMap();
		if (cmap == null) {
			LOGGER.log(Level.FINE, "Type 0 font dictionary doesn't contain Encoding");
			return null;
		}
		return cmap.getOrdering();
	}

    @Override
    /** Returns the descendant CIDSystemInfo registry, or null when it is absent. */
    public String getCIDFontRegistry() {
		PDCIDSystemInfo info = ((org.verapdf.pd.font.PDType0Font) this.pdFont).getCIDSystemInfo();
		if (info == null) {
			LOGGER.log(Level.FINE, "CID font dictionary doesn't contain CIDSystemInfo");
			return null;
		}
		return info.getStringKey(ASAtom.REGISTRY);
	}

    @Override
    /** Returns the CMap registry, or null when the Encoding map is absent. */
    public String getCMapRegistry() {
		org.verapdf.pd.font.cmap.PDCMap cmap = ((org.verapdf.pd.font.PDType0Font) this.pdFont).getCMap();
		if (cmap == null) {
			LOGGER.log(Level.FINE, "Type 0 font dictionary doesn't contain Encoding");
			return null;
		}
		return cmap.getRegistry();
	}

    @Override
    /** Returns the CID supplement, normalizing a missing numeric value to zero. */
    public Long getCIDFontSupplement() {
		PDCIDSystemInfo info = ((org.verapdf.pd.font.PDType0Font) this.pdFont).getCIDSystemInfo();
		if (info == null) {
			LOGGER.log(Level.FINE, "CID font dictionary doesn't contain CIDSystemInfo");
			return null;
		}
		Long supplement = info.getIntegerKey(ASAtom.SUPPLEMENT);
		return supplement != null ? supplement : 0L;
	}

    @Override
    /** Returns the CMap supplement, normalizing a missing numeric value to zero. */
    public Long getCMapSupplement() {
		org.verapdf.pd.font.cmap.PDCMap cmap = ((org.verapdf.pd.font.PDType0Font) this.pdFont).getCMap();
		if (cmap == null) {
			LOGGER.log(Level.FINE, "Type 0 font dictionary doesn't contain Encoding");
			return null;
		}
		Long supplement = cmap.getSupplement();
		return supplement != null ? supplement : 0L;
	}

    /** Returns the CMap name, using an empty string when no CMap is available. */
	@Override
	public String getcmapName() {
		org.verapdf.pd.font.cmap.PDCMap pdcMap = ((org.verapdf.pd.font.PDType0Font) this.pdFont).getCMap();
		if (pdcMap == null) {
			return "";
		}
		return pdcMap.getCMapName();
	}

    @Override
    /** Returns the descendant font dictionary's subtype name. */
    public String getSubtype() {
		return ((org.verapdf.pd.font.PDType0Font) this.pdFont).getType0FontDict().getNameKeyStringValue(ASAtom.SUBTYPE);
	}

    /** Compares CIDSystemInfo and CMap registries; source dictionaries are expected to exist. */
    public boolean isRegistryCompatible() {
		String fontRegistry = ((org.verapdf.pd.font.PDType0Font) this.pdFont).getCIDSystemInfo()
				.getStringKey(ASAtom.REGISTRY);
		String cMapRegistry = ((org.verapdf.pd.font.PDType0Font) this.pdFont).getCMap().getRegistry();
		if (fontRegistry == null) {
			LOGGER.log(Level.FINE, "CIDSystemInfo dictionary doesn't contain Registry entry.");
			return false;
		}
		return fontRegistry.equals(cMapRegistry);
	}

    /** Compares CIDSystemInfo and CMap orderings; source dictionaries are expected to exist. */
    public boolean isOrderingCompatible() {
		String fontOrdering = ((org.verapdf.pd.font.PDType0Font) this.pdFont).getCIDSystemInfo()
				.getStringKey(ASAtom.ORDERING);
		String cMapOrdering = ((org.verapdf.pd.font.PDType0Font) this.pdFont).getCMap().getOrdering();
		if (fontOrdering == null) {
			LOGGER.log(Level.FINE, "CIDSystemInfo dictionary doesn't contain Ordering entry.");
			return false;
		}
		return fontOrdering.equals(cMapOrdering);
	}
}
