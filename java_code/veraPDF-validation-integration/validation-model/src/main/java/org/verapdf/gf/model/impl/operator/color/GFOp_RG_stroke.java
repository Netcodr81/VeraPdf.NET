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
package org.verapdf.gf.model.impl.operator.color;

import org.verapdf.cos.COSBase;
import org.verapdf.model.operator.Op_RG_stroke;
import org.verapdf.model.pdlayer.PDColorSpace;

import java.util.List;

/**
 * RG sets stroking RGB from red, green, and blue operands (ISO 32000-1 section 8.6.8).
 *
 * <p> Its model type is {@code Op_RG_stroke} and it implements {@code Op_RG_stroke}. Operator objects are created while parsing content streams and are queried by validation profiles through their typed links.</p>
 *
 * <p><b>.NET porting notes:</b> Suggested C# type: {@code VeraPdf.Validation.Gf.Model.Impl.Operator.Color.GFOp_RG_stroke}, implementing {@code VeraPdf.Model.Operator.IOp_RG_stroke}. Expose links as {@code IReadOnlyList<IModelObject>} from {@code GetLinkedObjects(string)}; preserve nullable or empty links and map parser COS operands to {@code VeraPdf.Parser.Cos.CosValue} types. Java read-only {@code List} results should remain read-only in C#.</p>
 *
 * @author Maxim Plushchov
 */
public class GFOp_RG_stroke extends GFOpColor implements Op_RG_stroke {

    public static final String OP_RG_STROKE_TYPE = "Op_RG_stroke";

    public GFOp_RG_stroke(List<COSBase> arguments, PDColorSpace colorSpace) {
        super(arguments, colorSpace, OP_RG_STROKE_TYPE);
    }

}
