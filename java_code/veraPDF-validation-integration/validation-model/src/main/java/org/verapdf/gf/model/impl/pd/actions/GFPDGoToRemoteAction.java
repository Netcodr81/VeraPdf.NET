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

import org.verapdf.model.pdlayer.PDGoToRemoteAction;
import org.verapdf.pd.actions.PDAction;

/**
 * Models a GoToR action that targets a destination in another PDF file.
 *
 * <p>The action dictionary can name an external file and destination, making this
 * distinct from an in-document {@code /GoTo}. PDF/A rules inspect this active behavior
 * and its associated file specification; the adapter preserves the remote subtype
 * without opening or navigating to the target document.</p>
 *
 * <p><b>.NET porting notes:</b> Suggested type is
 * {@code VeraPdf.Validation.Gf.Model.Impl.Pd.Actions.GFPDGoToRemoteAction}, implementing
 * {@code VeraPdf.Model.PDLayer.IPDGoToRemoteAction}. Keep it as a subtype of the shared
 * action adapter and use the parser's PDF name/dictionary types rather than executing it.</p>
 *
 * @author Maksim Bezrukov
 */
public class GFPDGoToRemoteAction extends GFPDAction implements PDGoToRemoteAction {

	/** Model type name for a remote destination action. */
	public static final String GOTO_REMOTE_ACTION_TYPE = "PDGoToRemoteAction";

	/** Wraps a source action whose subtype is {@code /GoToR}. */
	public GFPDGoToRemoteAction(PDAction simplePDObject) {
		super(simplePDObject, GOTO_REMOTE_ACTION_TYPE);
	}
}
