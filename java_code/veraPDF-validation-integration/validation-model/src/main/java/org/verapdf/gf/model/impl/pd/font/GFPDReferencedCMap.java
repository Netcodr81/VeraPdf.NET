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

import org.verapdf.model.pdlayer.PDReferencedCMap;
import org.verapdf.pd.font.cmap.PDCMap;

/**
 * Marks a CMap reached through another map's {@code /UseCMap} entry.
 *
 * <p>Referenced CMaps participate in inherited character-code mappings; the subtype
 * lets profile rules distinguish the reference edge from the root encoding CMap while
 * reusing all name, embedded-stream, and further-inheritance logic of {@code GFPDCmap}.</p>
 *
 * <p><b>.NET porting notes:</b> Suggested type:
 * {@code VeraPdf.Validation.Gf.Model.Impl.Pd.Font.GFPDReferencedCMap}, implementing
 * {@code VeraPdf.Model.PDLayer.IPDReferencedCMap}. Reuse {@code PdfCMap} and preserve
 * recursive link semantics while guarding against cyclic inheritance.</p>
 *
 * @author Sergey Shemyakov
 */
public class GFPDReferencedCMap extends GFPDCmap implements PDReferencedCMap {

    /** Model type name for a referenced CMap. */
    public static final String REFERENCED_CMAP_TYPE = "PDReferencedCMap";

    /** Wraps the referenced parser CMap with its distinct model type. */
    public GFPDReferencedCMap(PDCMap pdcMap) {
        super(pdcMap, REFERENCED_CMAP_TYPE);
    }
}
