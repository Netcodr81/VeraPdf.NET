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

import org.verapdf.gf.model.impl.pd.GFPDObject;
import org.verapdf.model.baselayer.Object;
import org.verapdf.model.pdlayer.PDAction;
import org.verapdf.model.pdlayer.PDAdditionalActions;
import org.verapdf.pd.actions.PDAbstractAdditionalActions;

import java.util.ArrayList;
import java.util.List;

/**
 * Exposes the extra event-triggered actions attached to a PDF object.
 *
 * <p>Additional-actions dictionaries associate events such as page open, mouse enter,
 * or field focus with action dictionaries. These entries are relevant to interactive
 * behavior and are restricted by PDF/A; the adapter presents them as an {@code Actions}
 * link and retains the parent object's type for the validation profile. It is created
 * as part of the PD model and is consumed by rules that check permitted event/action
 * combinations.</p>
 *
 * <p><b>.NET porting notes:</b></p>
 * <ul>
 *   <li>Suggested C# type: {@code VeraPdf.Validation.Gf.Model.Impl.Pd.Actions.GFPDAdditionalActions},
 *       implementing {@code VeraPdf.Model.PDLayer.IPDAdditionalActions}.</li>
 *   <li>Use {@code IReadOnlyList<IPDAction>} for the action link and keep the source
 *       {@code PDAbstractAdditionalActions} as a parser-facing adapter boundary.</li>
 *   <li>Translate the Java cast to a checked C# pattern and preserve source ordering;
 *       nullable model values should remain nullable.</li>
 * </ul>
 *
 * @author Maxim Plushchov
 */
public class GFPDAdditionalActions extends GFPDObject implements PDAdditionalActions {

    /** Model type name for an event-to-action dictionary. */
    public static final String ADDITIONAL_ACTIONS_TYPE = "PDAdditionalActions";

    /** Link name used by validation rules to enumerate event actions. */
    public static final String ACTIONS = "Actions";

    /** Wraps the parser's additional-actions dictionary. */
    public GFPDAdditionalActions(PDAbstractAdditionalActions additionalActions) {
        super(additionalActions, ADDITIONAL_ACTIONS_TYPE);
    }

    @Override
    /** Resolves the supported {@code Actions} link, delegating other links to the base. */
    public List<? extends Object> getLinkedObjects(String link) {
        switch (link) {
            case ACTIONS:
                return getActions();
            default:
                return super.getLinkedObjects(link);
        }
    }

    /** Converts each raw action to its subtype-specific model adapter. */
    private List<PDAction> getActions() {
        List<org.verapdf.pd.actions.PDAction> actions = ((PDAbstractAdditionalActions)simplePDObject).getActions();
        List<PDAction> pdActions = new ArrayList<>(actions.size());
        for (org.verapdf.pd.actions.PDAction action : actions) {
            this.addAction(pdActions, action);
        }
        return pdActions;
    }

    /** Adds an action only when the factory can produce a non-null adapter. */
    private void addAction(List<PDAction> actions, org.verapdf.pd.actions.PDAction raw) {
        PDAction action = GFPDAction.getAction(raw);
        if (action != null) {
            actions.add(action);
        }
    }

    @Override
    /** Returns the PDF object type that owns this additional-actions dictionary. */
    public String getparentType() {
        return ((PDAbstractAdditionalActions)simplePDObject).getParentType();
    }
}
