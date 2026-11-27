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
package org.verapdf.gf.model.impl.operator.markedcontent;

import org.verapdf.cos.COSBase;
import org.verapdf.pd.PDResourcesHandler;
import org.verapdf.model.baselayer.Object;
import org.verapdf.model.operator.Op_DP;

import java.util.Collections;
import java.util.List;

/**
 * DP attaches a tag and property list at a point in the content stream (ISO 32000-1 section 14.6.2). Unlike BDC, it is not a sequence opener and must not be pushed on the nesting stack.
 *
 * <p> The model type is {@code Op_DP} implementing {@code Op_DP}. The parser creates these objects while walking the content stream; validation profiles query their typed links.</p>
 *
 * <p><b>.NET porting notes:</b> Suggested C# type: {@code VeraPdf.Validation.Gf.Model.Impl.Operator.MarkedContent.GFOp_DP}, implementing {@code VeraPdf.Model.Operator.IOp_DP}. Expose links through {@code IReadOnlyList<IModelObject> GetLinkedObjects(string)} and keep missing data as empty/null links. Map Java lists to read-only .NET collections and parser COS data to {@code VeraPdf.Parser.Cos.CosValue} types. Resource/parent-tree context should remain document-scoped rather than static.</p>
 *
 * @author Timur Kamalov
 */
public class GFOp_DP extends GFOpMarkedContent implements Op_DP {

	/** Type name for {@code GFOp_DP} */
    public static final String OP_DP_TYPE = "Op_DP";

    public GFOp_DP(List<COSBase> arguments, PDResourcesHandler resources) {
		super(arguments, OP_DP_TYPE, null, Collections.emptyList(), false);
		initializePropertiesDict(resources);
    }

	@Override
	public List<? extends Object> getLinkedObjects(
			String link) {
		switch (link) {
			case TAG:
				return this.getLinkTag();
			case PROPERTIES:
				return this.getPropertiesDict();
			default:
				return super.getLinkedObjects(link);
		}
	}

}
