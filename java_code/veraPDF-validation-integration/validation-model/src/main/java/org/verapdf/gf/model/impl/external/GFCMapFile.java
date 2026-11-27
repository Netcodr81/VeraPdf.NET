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

import org.verapdf.cos.COSStream;
import org.verapdf.model.external.CMapFile;

import java.util.logging.Logger;

/**
 * Represents an embedded character map (CMap) program and reports its writing mode and CID range.
 *
 * <p>A CMap maps character codes to character identifiers and may define horizontal or vertical writing. The
 * stream is parsed by the PDF font subsystem; these properties support font and text validation without exposing
 * the whole CMap program as model links.</p>
 *
 * <p><b>.NET porting notes:</b> Implement {@code VeraPdf.Model.External.ICMapFile} and port the parser from
 * {@code org.verapdf.pd.font.cmap.CMapFile}; there is no direct .NET CMap equivalent in {@code VeraPdf.Parser}
 * yet. Reuse the parser's decoded stream handling and expose count-like results as nullable {@code long?}.</p>
 *
 * @author Sergey Shemyakov
 */
public class GFCMapFile extends GFExternal implements CMapFile {

    private static final Logger LOGGER = Logger.getLogger(GFCMapFile.class.getCanonicalName());

    private final org.verapdf.pd.font.cmap.CMapFile cMapFile;

    /**
     * Type name for GFCMapFile
     */
    public static final String CMAP_FILE_TYPE = "CMapFile";

    /**
     * Constructor from COSStream containing CMap.
     *
     * @param stream is CMap stream.
     */
    public GFCMapFile(COSStream stream) {
        super(CMAP_FILE_TYPE);
        this.cMapFile = new org.verapdf.pd.font.cmap.CMapFile(stream);
    }

    /**
     * @return the value of the WMode entry in the embedded CMap file.
     */
    @Override
    public Long getWMode() {
        return (long) cMapFile.getWMode();
    }

    /**
     * @return the value of the WMode entry in the parent CMap dictionary.
     */
    @Override
    public Long getdictWMode() {
        return (long) cMapFile.getDictWMode();
    }

    @Override
    public Long getmaximalCID() {
        return (long) this.cMapFile.getMaxCID();
    }
}
