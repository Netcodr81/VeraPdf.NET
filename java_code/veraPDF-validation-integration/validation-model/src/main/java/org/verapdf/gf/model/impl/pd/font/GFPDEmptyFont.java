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
package org.verapdf.gf.model.impl.pd.font;

import org.verapdf.gf.model.factory.operators.RenderingMode;
import org.verapdf.model.baselayer.Object;

import java.util.List;

/**
 * Sentinel font used when font resolution cannot produce a concrete PDF font.
 *
 * <p>The adapter supplies a stable model object with a neutral rendering mode, allowing
 * callers to continue traversing a document without treating a missing font as a real
 * font dictionary. It adds no font-specific links beyond those of the generic font base.</p>
 *
 * <p><b>.NET porting notes:</b> Suggested type:
 * {@code VeraPdf.Validation.Gf.Model.Impl.Pd.Font.GFPDEmptyFont}. Keep the sentinel distinct
 * from a parsed {@code PdfFont}; nullable fields and empty link collections must not imply
 * that a valid font program exists.</p>
 *
 * @author Sergey Shemyakov
 */
public class GFPDEmptyFont extends GFPDFont {

    /** Creates a null-backed font sentinel with no rendering behavior. */
    public GFPDEmptyFont() {
        super(null, RenderingMode.NEITHER, null, RESOURCE_TYPE);
    }

    @Override
    /** The sentinel contributes no font-specific links. */
    public List<? extends Object> getLinkedObjects(String link) {
        return super.getLinkedObjects(link);
    }
}
