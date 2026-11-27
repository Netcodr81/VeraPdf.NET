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
package org.verapdf.gf.model.impl.operator.generalgs;

import org.verapdf.cos.COSBase;
import org.verapdf.model.baselayer.Object;
import org.verapdf.model.coslayer.CosNumber;
import org.verapdf.model.operator.Op_w_line_width;

import java.util.List;

/**
 * w sets the path stroke width (ISO 32000-1 section 8.4.3). Zero requests a device hairline and must not be clamped to a positive width.
 *
 * <p> The model type is {@code Op_w_line_width} implementing {@code Op_w_line_width}. Named links include {@code lineWidth}. The parser creates these objects while walking the content stream; validation profiles query their typed links.</p>
 *
 * <p><b>.NET porting notes:</b> Suggested C# type: {@code VeraPdf.Validation.Gf.Model.Impl.Operator.GeneralGS.GFOp_w_line_width}, implementing {@code VeraPdf.Model.Operator.IOp_w_line_width}. Expose links through {@code IReadOnlyList<IModelObject> GetLinkedObjects(string)} and keep missing data as empty/null links. Map Java lists to read-only .NET collections and parser COS data to {@code VeraPdf.Parser.Cos.CosValue} types. Resource/parent-tree context should remain document-scoped rather than static.</p>
 *
 * @author Timur Kamalov
 */
public class GFOp_w_line_width extends GFOpGeneralGS
		implements Op_w_line_width {

	/** Type name for {@code GFOp_w_line_width} */
	public static final String OP_W_LINE_WIDTH_TYPE = "Op_w_line_width";

	/** Name of link to the width for */
    public static final String LINE_WIDTH = "lineWidth";

    public GFOp_w_line_width(List<COSBase> arguments) {
        super(arguments, OP_W_LINE_WIDTH_TYPE);
    }

    @Override
    public List<? extends Object> getLinkedObjects(String link) {
        if (LINE_WIDTH.equals(link)) {
            return this.getLineWidth();
        }
        return super.getLinkedObjects(link);
    }

    private List<CosNumber> getLineWidth() {
        return this.getLastNumber();
    }

}
