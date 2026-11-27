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
import org.verapdf.cos.COSObject;
import org.verapdf.gf.model.impl.pd.GFPDObject;
import org.verapdf.model.pdlayer.PDMediaClip;

/**
 * Exposes metadata from a media clip referenced by a rendition action.
 *
 * <p>A media clip dictionary describes the content type and alternate description
 * for embedded media. The alternate text is important to accessible presentation,
 * while media actions are active content that PDF/A validators may prohibit. This
 * adapter reads values from the parser's PD action package for rules checking both
 * the descriptor and its alternate text.</p>
 *
 * <p><b>.NET porting notes:</b> Implement
 * {@code VeraPdf.Model.PDLayer.IPDMediaClip} as
 * {@code VeraPdf.Validation.Gf.Model.Impl.Pd.Actions.GFPDMediaClip}. Use
 * {@code VeraPdf.Parser.Cos.CosDictionary} / {@code CosName} for raw keys and preserve
 * missing {@code /Alt} as {@code null}; Java's generic COS string conversion may need
 * explicit decoding in C#.</p>
 *
 * @author Maxim Plushchov
 */
public class GFPDMediaClip extends GFPDObject implements PDMediaClip {

    /** Model type name for a rendition media clip. */
    public static final String MEDIA_CLIP_TYPE = "PDMediaClip";

    /** Creates the adapter around the parser's media clip dictionary. */
    public GFPDMediaClip(org.verapdf.pd.actions.PDMediaClip simplePDObject) {
        super(simplePDObject, MEDIA_CLIP_TYPE);
    }

    @Override
    /** Returns the declared MIME content type for the media data. */
    public String getCT() {
        return ((org.verapdf.pd.actions.PDMediaClip)simplePDObject).getContentType();
    }

    @Override
    /** Returns the raw alternate-description entry, or null when it is absent. */
    public String getAlt() {
        COSObject object = simplePDObject.getKey(ASAtom.ALT);
        return object != null ? object.toString() : null;
    }

    @Override
    /** Reports whether the alternate description satisfies the parser's checks. */
    public Boolean gethasCorrectAlt() {
        return ((org.verapdf.pd.actions.PDMediaClip)simplePDObject).hasCorrectAlt();
    }
}
