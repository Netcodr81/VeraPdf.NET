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
package org.verapdf.gf.model.impl.cos;

import org.verapdf.cos.COSName;
import org.verapdf.model.coslayer.CosRenderingIntent;

/**
 * Represents a PDF rendering-intent name as a specialized COS name model object.
 *
 * <p>Rendering intent selects how device-independent colors are mapped to an output device (ISO 32000).
 * The subtype constructor lets derived adapters retain a more specific model type while sharing name decoding.</p>
 *
 * <p><b>.NET porting notes:</b> Implement {@code VeraPdf.Model.CosLayer.ICosRenderingIntent} on the COS-name
 * adapter backed by {@code VeraPdf.Parser.Cos.CosName}; retain the exact rendering-intent names and do not
 * replace their PDF-defined semantics with a platform graphics enum.</p>
 *
 * @author Sergey Shemyakov
 */
public class GFCosRenderingIntent extends GFCosName implements CosRenderingIntent {

    /** Type name for GFCosRenderingIntent */
    public static final String COS_RENDERING_INTENT_TYPE = "CosRenderingIntent";

    /**
     * Default constructor
     * @param cosName is rendering intent COSName.
     */
    public GFCosRenderingIntent(COSName cosName) {
        this(cosName, COS_RENDERING_INTENT_TYPE);
    }

    /**
     * Constructor for child classes
     * @param cosName is rendering intent COSName.
     * @param type child class type.
     */
    public GFCosRenderingIntent(COSName cosName, final String type) {
        super(cosName, type);
    }
}
