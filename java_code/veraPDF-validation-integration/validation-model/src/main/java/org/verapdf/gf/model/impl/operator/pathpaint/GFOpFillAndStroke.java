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
import org.verapdf.model.baselayer.Object;

import java.util.List;

/**
 * Base for path operators that both fill and stroke (ISO 32000-1 section 8.5.3). Fill and stroke color spaces are resolved separately using their respective overprint flags.
 *
 * <p> This is an operator-family base. The operator parser supplies inherited graphics state; validation profiles inspect the operator type and links.</p>
 *
 * <p><b>.NET porting notes:</b> Suggested C# type: {@code VeraPdf.Validation.Gf.Model.Impl.Operator.PathPaint.GFOpFillAndStroke}, implementing {@code VeraPdf.Model.Operator.IModelObject}. Use {@code IReadOnlyList<IModelObject>} for {@code GetLinkedObjects(string)} and preserve optional fill/stroke color-space links. Resolve color spaces through the ported model factory rather than assuming a drawing-library color representation.</p>
 *
 * @author Timur Kamalov
 */
public abstract class GFOpFillAndStroke extends GFOpPathPaint {

	protected GFOpFillAndStroke(List<COSBase> arguments, final GraphicState state,
								final PDResourcesHandler resourcesHandler, final String opType) {
		super(arguments, state.isProcessColorOperators(), opType);
		this.fillCS = getColorSpace(state, resourcesHandler, state.getFillColorSpace(), 
				state.isOverprintingFlagNonStroke());
		this.strokeCS = getColorSpace(state, resourcesHandler, state.getStrokeColorSpace(), 
				state.isOverprintingFlagStroke());
	}

	@Override
	public List<? extends Object> getLinkedObjects(
			String link) {
		switch (link) {
			case STROKE_CS:
				return this.getStrokeCS();
			case FILL_CS:
				return this.getFillCS();
			default:
				return super.getLinkedObjects(link);
		}
	}

}
