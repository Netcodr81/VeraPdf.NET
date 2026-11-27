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
import org.verapdf.cos.COSObjType;
import org.verapdf.cos.COSObject;
import org.verapdf.model.baselayer.Object;
import org.verapdf.model.pdlayer.PDMediaClip;
import org.verapdf.model.pdlayer.PDRenditionAction;
import org.verapdf.pd.actions.PDAction;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

/**
 * Models a rendition action and exposes its associated media clip, when present.
 *
 * <p>Rendition actions link an event to a media rendition; the code follows {@code /R}
 * and then its {@code /C} entry, wrapping a dictionary as a {@code PDMediaClip}. This
 * data describes potentially executable multimedia, which is restricted in archival
 * PDFs and should not be played during validation. The adapter exposes the nested clip
 * through the {@code C} model link.</p>
 *
 * <p><b>.NET porting notes:</b></p>
 * <ul>
 *   <li>Suggested type: {@code VeraPdf.Validation.Gf.Model.Impl.Pd.Actions.GFPDRenditionAction},
 *       implementing {@code VeraPdf.Model.PDLayer.IPDRenditionAction}.</li>
 *   <li>Use {@code IReadOnlyList<IModelObject>} and parser-side {@code CosDictionary};
 *       check both missing values and the dictionary kind before constructing the clip.</li>
 *   <li>Replace {@code Collections.emptyList()} / unmodifiable views with
 *       {@code Array.Empty<IModelObject>()} / read-only collections.</li>
 * </ul>
 *
 * @author Maxim Plushchov
 */
public class GFPDRenditionAction extends GFPDAction implements PDRenditionAction {

    /** Model type name for a rendition action. */
    public static final String RENDITION_ACTION_TYPE = "PDRenditionAction";

    /** Link name to the rendition media clip. */
    public static final String C = "C";

    /** Wraps a source action with the rendition-specific model type. */
    public GFPDRenditionAction(PDAction simplePDObject) {
        super(simplePDObject, RENDITION_ACTION_TYPE);
    }

    @Override
    /** Resolves {@code C} to the optional clip and delegates other links to the base. */
    public List<? extends Object> getLinkedObjects(String link) {
        switch (link) {
            case C:
                return this.getC();
            default:
                return super.getLinkedObjects(link);
        }
    }

    /** Follows the rendition dictionary's {@code /C} value if it is a dictionary. */
    private List<PDMediaClip> getC() {
        COSObject object = ((PDAction) this.simplePDObject).getRendition();
        if (object != null) {
            object = object.getKey(ASAtom.C);
            if (object != null && object.getType() == COSObjType.COS_DICT) {
                List<PDMediaClip> list = new ArrayList<>(MAX_NUMBER_OF_ELEMENTS);
                list.add(new GFPDMediaClip(new org.verapdf.pd.actions.PDMediaClip(object)));
                return Collections.unmodifiableList(list);
            }
        }
        return Collections.emptyList();
    }

}
