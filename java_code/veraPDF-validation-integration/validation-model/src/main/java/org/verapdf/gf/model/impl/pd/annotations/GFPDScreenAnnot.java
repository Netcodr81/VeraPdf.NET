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
package org.verapdf.gf.model.impl.pd.annotations;

import org.verapdf.gf.model.impl.pd.GFPDAnnot;
import org.verapdf.pd.PDResourcesHandler;
import org.verapdf.model.pdlayer.PDScreenAnnot;
import org.verapdf.pd.PDAnnotation;
import org.verapdf.pd.PDPage;

/**
 * Represents a screen annotation that associates a page region with media playback.
 *
 * <p>The screen subtype is associated with media actions and may include a movie or
 * sound rendition. As active content it is subject to PDF/A restrictions; this class
 * provides the annotation subtype for rules and inherits shared annotation fields.</p>
 *
 * <p><b>.NET porting notes:</b> Suggested type:
 * {@code VeraPdf.Validation.Gf.Model.Impl.Pd.Annotations.GFPDScreenAnnot}, implementing
 * {@code VeraPdf.Model.PDLayer.IPDScreenAnnot}; preserve parser-owned source data and
 * never start playback while validating.</p>
 *
 * @author Maxim Plushchov
 */
public class GFPDScreenAnnot extends GFPDAnnot implements PDScreenAnnot {

	/** Model type name for screen annotations. */
	public static final String SCREEN_ANNOTATION_TYPE = "PDScreenAnnot";

	public GFPDScreenAnnot(PDAnnotation annot, PDResourcesHandler pageResources, PDPage page) {
		super(annot, pageResources, page, SCREEN_ANNOTATION_TYPE);
	}

}
