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
package org.verapdf.gf.model.impl.pd;

import org.verapdf.model.pdlayer.PDDestination;

import java.util.logging.Logger;

/**
 * Validation-model representation of a PDF destination, such as an outline or link target.
 *
 * <p>PDF destinations identify a page and optional view parameters or a structure element.  This
 * adapter currently exposes whether the destination is structure-based for PDF/UA checks.</p>
 *
 * <p><b>.NET porting notes:</b></p>
 * <ul>
 *   <li>Suggested C# type: {@code VeraPdf.Validation.Gf.Model.Impl.Pd.GFPDDestination}, implementing
 *       {@code VeraPdf.Model.PDLayer.IPDDestination}.</li>
 *   <li>Map {@code org.verapdf.pd.PDDestination} to a future
 *       {@code VeraPdf.Parser.Pd.PdfDestination}; keep the resulting {@code bool?} nullable contract.</li>
 * </ul>
 *
 * @author Maxim Plushchov
 */
public class GFPDDestination extends GFPDObject implements PDDestination {

    public static final Logger LOGGER = Logger.getLogger(GFPDDestination.class.getCanonicalName());

    public static final String DESTINATION_TYPE = "PDDestination";

    public GFPDDestination(org.verapdf.pd.PDDestination destination) {
        super(destination, DESTINATION_TYPE);
    }

    @Override
    public Boolean getisStructDestination() {
        return ((org.verapdf.pd.PDDestination)simplePDObject).getIsStructDestination();
    }
}
