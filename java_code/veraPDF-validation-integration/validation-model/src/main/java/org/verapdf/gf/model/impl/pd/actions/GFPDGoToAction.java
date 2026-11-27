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
package org.verapdf.gf.model.impl.pd.actions;

import org.verapdf.model.pdlayer.PDGoToAction;
import org.verapdf.pd.actions.PDAction;


/**
 * Models an internal GoTo action that navigates to a destination in this document.
 *
 * <p>The destination may identify a page structurally, which is why this adapter
 * exposes whether it contains a structure destination. Such navigation is distinct
 * from a remote GoTo action and is inspected, never performed, by validation rules.
 * The PD action factory creates this subclass when {@code /S} is {@code /GoTo}.</p>
 *
 * <p><b>.NET porting notes:</b> Implement {@code IPDGoToAction} as
 * {@code VeraPdf.Validation.Gf.Model.Impl.Pd.Actions.GFPDGoToAction}; retain the
 * nullable {@code bool?} result and map parser-side action access through
 * {@code VeraPdf.Parser.Cos} dictionaries or the parser's PD action abstraction.</p>
 *
 * @author Maksim Bezrukov
 */
public class GFPDGoToAction extends GFPDAction implements PDGoToAction {
	/** Model type name for an internal destination action. */
	public static final String GOTO_ACTION_TYPE = "PDGoToAction";

	/** Creates the specialized adapter while preserving the GoTo model type. */
	public GFPDGoToAction(PDAction simplePDObject) {
		super(simplePDObject, GOTO_ACTION_TYPE);
	}

	@Override
	/** Reports whether the destination points into the tagged structure tree. */
	public Boolean getcontainsStructDestination() {
		return ((PDAction) simplePDObject).containsStructureDestination();
	}
}
