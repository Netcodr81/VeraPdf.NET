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
package org.verapdf.gf.model.impl.operator.pathpaint;

import org.verapdf.cos.COSBase;
import org.verapdf.gf.model.factory.operators.GraphicState;
import org.verapdf.pd.PDResourcesHandler;
import org.verapdf.model.operator.Op_b_closepath_fill_stroke;

import java.util.List;

/**
 * b closes the current subpath, fills using the nonzero winding rule, then strokes (ISO 32000-1 section 8.5.3).
 *
 * <p> The model type is {@code Op_b_closepath_fill_stroke} implementing {@code Op_b_closepath_fill_stroke}. The operator parser supplies inherited graphics state; validation profiles inspect the operator type and links.</p>
 *
 * <p><b>.NET porting notes:</b> Suggested C# type: {@code VeraPdf.Validation.Gf.Model.Impl.Operator.PathPaint.GFOp_b_closepath_fill_stroke}, implementing {@code VeraPdf.Model.Operator.IOp_b_closepath_fill_stroke}. Use {@code IReadOnlyList<IModelObject>} for {@code GetLinkedObjects(string)} and preserve optional fill/stroke color-space links. Resolve color spaces through the ported model factory rather than assuming a drawing-library color representation.</p>
 *
 * @author Timur Kamalov
 */
public class GFOp_b_closepath_fill_stroke extends GFOpFillAndStroke
		implements Op_b_closepath_fill_stroke {

	/** Type name for {@code GFOp_b_closepath_fill_stroke} */
	public static final String OP_B_CLOSEPATH_FILL_STROKE_TYPE = "Op_b_closepath_fill_stroke";

	public GFOp_b_closepath_fill_stroke(List<COSBase> arguments, final GraphicState state,
										final PDResourcesHandler resourcesHandler) {
		super(arguments, state, resourcesHandler, OP_B_CLOSEPATH_FILL_STROKE_TYPE);
	}

}
