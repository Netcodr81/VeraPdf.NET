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

import org.verapdf.cos.COSObjType;
import org.verapdf.cos.COSObject;
import org.verapdf.cos.COSStream;
import org.verapdf.gf.model.impl.external.GFCMapFile;
import org.verapdf.gf.model.impl.pd.GFPDObject;
import org.verapdf.model.baselayer.Object;
import org.verapdf.model.external.CMapFile;
import org.verapdf.model.pdlayer.PDCMap;
import org.verapdf.model.pdlayer.PDReferencedCMap;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

/**
 * Adapts a font CMap, which maps encoded character codes to character identifiers or Unicode.
 *
 * <p>A CMap may be a predefined name or an embedded stream and can inherit from another
 * map through {@code /UseCMap}. Type0 fonts use this information to decode multibyte
 * character codes and map them to descendant CIDFont glyphs. The model exposes the
 * optional embedded CMap program and parent map as named links.</p>
 *
 * <p><b>.NET porting notes:</b> Suggested type:
 * {@code VeraPdf.Validation.Gf.Model.Impl.Pd.Font.GFPDCmap}, implementing
 * {@code VeraPdf.Model.PDLayer.IPDCMap}. The parser counterpart is
 * {@code VeraPdf.Parser.Fonts.PdfCMap}; use {@code CMapParser} for parsing and expose links
 * as read-only collections. CMap inheritance can be recursive, so preserve cycle handling.</p>
 *
 * @author Sergey Shemyakov
 */
public class GFPDCmap extends GFPDObject implements PDCMap {

    /** Model type name for a CMap. */
    public static final String CMAP_TYPE = "PDCMap";
    /** Link name for an embedded CMap stream adapter. */
    public static final String EMBEDDED_FILE = "embeddedFile";
    /** Link name for the map referenced by {@code /UseCMap}. */
    public static final String USE_C_MAP = "UseCMap";

    /** Creates the standard CMap model wrapper. */
    public GFPDCmap(org.verapdf.pd.font.cmap.PDCMap pdcMap) {
        super(pdcMap, CMAP_TYPE);
    }

    /** Creates a wrapper with a more specific model type, such as a referenced CMap. */
    public GFPDCmap(org.verapdf.pd.font.cmap.PDCMap pdcMap, String type) {
        super(pdcMap, type);
    }

    /** Returns the embedded or predefined CMap name. */
    @Override
    public String getCMapName() {
        return this.pdcMap.getCMapName();
    }

    @Override
    /** Dispatches the embedded-file and inherited-map links. */
    public List<? extends Object> getLinkedObjects(String link) {
        switch (link) {
            case EMBEDDED_FILE:
                return this.getEmbeddedFile();
            case USE_C_MAP:
                return this.getUseCMap();
            default:
                return super.getLinkedObjects(link);
        }
    }

    /** Wraps a CMap stream as an external file only when the source value is a stream. */
    private List<CMapFile> getEmbeddedFile() {
        if (this.pdcMap.getcMap().getType() == COSObjType.COS_STREAM) {
            List<CMapFile> result = new ArrayList<>(MAX_NUMBER_OF_ELEMENTS);
            result.add(new GFCMapFile((COSStream) this.pdcMap.getcMap().getDirectBase()));
            return Collections.unmodifiableList(result);
        }
        return Collections.emptyList();
    }
    
    @Override
    /** Reports whether the CMap definition is stored in a stream. */
    public Boolean getcontainsEmbeddedFile() {
        return this.pdcMap.getcMap().getType() == COSObjType.COS_STREAM;
    }

    /** Wraps the optional {@code /UseCMap} value as a referenced-CMap model object. */
    private List<PDReferencedCMap> getUseCMap() {
        COSObject useCMap = this.pdcMap.getUseCMap();
        if (useCMap.empty()) {
            return Collections.emptyList();
        }
        List<PDReferencedCMap> list = new ArrayList<>(MAX_NUMBER_OF_ELEMENTS);
        list.add(new GFPDReferencedCMap(new org.verapdf.pd.font.cmap.PDCMap(useCMap)));
        return Collections.unmodifiableList(list);
    }
}
