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
import org.verapdf.model.coslayer.CosBM;

/**
 * Wraps the PDF blend-mode name as a specialized COS model value.
 *
 * <p>Blend modes control how transparent graphics are composited with the backdrop (ISO 32000). The underlying
 * object remains a PDF name, while the distinct {@code CosBM} model type lets transparency rules target it.</p>
 *
 * <p><b>.NET porting notes:</b> Implement {@code VeraPdf.Validation.Gf.Model.Impl.Cos.GFCosBM} as
 * {@code VeraPdf.Model.CosLayer.ICosBM}, deriving from the name adapter and backed by
 * {@code VeraPdf.Parser.Cos.CosName}; retain the exact model type name.</p>
 *
 * @author Maxim Plushchov
 */
public class GFCosBM extends GFCosName implements CosBM {

    public static final String COS_BM_TYPE = "CosBM";

    public GFCosBM(COSName cosName) {
        super(cosName, COS_BM_TYPE);
    }

}
