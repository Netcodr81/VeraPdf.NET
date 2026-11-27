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

import org.verapdf.as.ASAtom;
import org.verapdf.model.pdlayer.PDNamedAction;
import org.verapdf.pd.actions.PDAction;

/**
 * Models an action whose behavior is selected by a standard PDF name.
 *
 * <p>Named actions use {@code /S /Named} and store the requested operation in {@code /N},
 * for example a viewer navigation command. The action is represented as data for
 * validation rather than invoked by the application; PDF/A profiles can therefore
 * detect prohibited interactive behavior.</p>
 *
 * <p><b>.NET porting notes:</b> Suggested type is
 * {@code VeraPdf.Validation.Gf.Model.Impl.Pd.Actions.GFPDNamedAction}, implementing
 * {@code VeraPdf.Model.PDLayer.IPDNamedAction}. Map the PDF name through
 * {@code VeraPdf.Parser.Cos.CosName} and retain a nullable string when {@code /N} is missing.</p>
 *
 * @author Maksim Bezrukov
 */
public class GFPDNamedAction extends GFPDAction implements PDNamedAction {
	/** Model type name for a named action. */
	public static final String NAMED_ACTION_TYPE = "PDNamedAction";

	/** Wraps the source named action dictionary. */
	public GFPDNamedAction(PDAction simplePDObject) {
		super(simplePDObject, NAMED_ACTION_TYPE);
	}

	@Override
	/** Returns the operation name from the action's {@code /N} entry. */
	public String getN() {
		ASAtom n =((PDAction) this.simplePDObject).getN();
		return n == null ? null : n.getValue();
	}
}
