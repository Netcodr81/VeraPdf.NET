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
import org.verapdf.gf.model.impl.pd.GFPDObject;
import org.verapdf.model.baselayer.Object;
import org.verapdf.model.pdlayer.PDAction;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

/**
 * Adapts a PDF action dictionary into the validation model's {@code PDAction} object.
 *
 * <p>PDF actions describe interactive behavior such as navigating to a destination,
 * opening a remote document, or presenting media; the action subtype is stored in
 * {@code /S}, and {@code /Next} can chain additional actions. PDF/A generally forbids
 * or restricts active content, so validators inspect this graph rather than executing
 * it. veraPDF creates this adapter while traversing the document-level PD model, and
 * profile rules read its subtype and follow its named {@code Next} link.</p>
 *
 * <p><b>.NET porting notes:</b></p>
 * <ul>
 *   <li>Suggested C# type: {@code VeraPdf.Validation.Gf.Model.Impl.Pd.Actions.GFPDAction},
 *       implementing {@code VeraPdf.Model.PDLayer.IPDAction}.</li>
 *   <li>Adapt the source {@code org.verapdf.pd.actions.PDAction} with parser/model
 *       abstractions; use {@code VeraPdf.Parser.Cos.CosDictionary} and
 *       {@code CosName} if reading the action dictionary directly.</li>
 *   <li>Use {@code IReadOnlyList<IModelObject>} for links and a subtype factory for
 *       dispatch. Preserve absent subtype as {@code null}; do not execute actions.</li>
 * </ul>
 *
 * @author Maksim Bezrukov
 */
public class GFPDAction extends GFPDObject implements PDAction {

	/** Model type name used to group generic PDF actions in validation profiles. */
	public static final String ACTION_TYPE = "PDAction";

	/** Link name through which chained actions are exposed to profile rules. */
	public static final String NEXT = "Next";

	public static final String NAMED_SUBTYPE = "Named";
	public static final String GO_TO_SUBTYPE = "GoTo";
	public static final String GO_TO_R_SUBTYPE = "GoToR";
	public static final String RENDITION_SUBTYPE = "Rendition";

	/** Creates an adapter with the generic action model type. */
	public GFPDAction(
			org.verapdf.pd.actions.PDAction simplePDObject) {
		this(simplePDObject, ACTION_TYPE);
	}

	/** Creates an adapter using a subtype-specific model type name. */
	public GFPDAction(
			org.verapdf.pd.actions.PDAction simplePDObject,
			final String type) {
		super(simplePDObject, type);
	}

	@Override
	/** Returns the value of the action dictionary's {@code /S} subtype name. */
	public String getS() {
		ASAtom subtype =((org.verapdf.pd.actions.PDAction) simplePDObject)
				.getSubtype();
		return subtype == null ? null : subtype.getValue();
	}

	@Override
	/** Dispatches the {@code Next} link and delegates unknown links to the base PD object. */
	public List<? extends Object> getLinkedObjects(String link) {
		if (NEXT.equals(link)) {
			return this.getNext();
		}
		return super.getLinkedObjects(link);
	}

	/** Wraps chained actions, omitting null adapters and exposing an immutable list. */
	private List<PDAction> getNext() {
		List<org.verapdf.pd.actions.PDAction> nextActionList =
				((org.verapdf.pd.actions.PDAction) this.simplePDObject)
						.getNext();
		if (!nextActionList.isEmpty()) {
			List<PDAction> actions = new ArrayList<>(nextActionList.size());
			for (org.verapdf.pd.actions.PDAction action : nextActionList) {
				PDAction result = getAction(action);
				if (result != null) {
					actions.add(result);
				}
			}
			return Collections.unmodifiableList(actions);
		}
		return Collections.emptyList();
	}

	/** Selects the specialized adapter from the source action subtype; null stays null. */
	public static PDAction getAction(org.verapdf.pd.actions.PDAction action) {
		if (action == null) {
			return null;
		}
		ASAtom subtype = action.getSubtype();
		if (subtype == null) {
			return new GFPDAction(action);
		}

		switch (subtype.getValue()) {
			case NAMED_SUBTYPE:
				return new GFPDNamedAction(action);
			case GO_TO_SUBTYPE:
				return new GFPDGoToAction(action);
			case GO_TO_R_SUBTYPE:
				return new GFPDGoToRemoteAction(action);
			case RENDITION_SUBTYPE:
				return new GFPDRenditionAction(action);
			default:
				return new GFPDAction(action);
		}
	}
}
